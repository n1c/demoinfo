using DemoInfo.DP.Handler;
using DemoInfo.DT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoInfo.DP
{
    internal class Entity
    {
        public int ID { get; set; }

        public ServerClass ServerClass { get; set; }

        public PropertyEntry[] Props { get; private set; }

        public Entity(int id, ServerClass serverClass)
        {
            this.ID = id;
            this.ServerClass = serverClass;

            List<FlattenedPropEntry> flattenedProps = ServerClass.FlattenedProps;
            Props = new PropertyEntry[flattenedProps.Count];
            for (int i = 0; i < flattenedProps.Count; i++)
                Props[i] = new PropertyEntry(flattenedProps[i], i);
        }

        public PropertyEntry FindProperty(string name)
        {
            return Props.Single(a => a.Entry.PropertyName == name);
        }

        /// <summary>
        /// Applies the update.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public void ApplyUpdate(IBitStream reader)
        {
            //Okay, how does an entity-update look like?
            //First a list of the updated props is sent
            //And then the props itself are sent.

            //Read the field-indicies in a "new" way?
            bool newWay = reader.ReadBit();
            int index = -1;
            var entries = new List<PropertyEntry>();

            //No read them.
            while ((index = ReadFieldIndex(reader, index, newWay)) != -1)
                entries.Add(this.Props[index]);

            //Now read the updated props
            foreach (var prop in entries)
            {
                prop.Decode(reader, this);
            }
        }

        int ReadFieldIndex(IBitStream reader, int lastIndex, bool bNewWay)
        {
            if (bNewWay)
            {
                if (reader.ReadBit())
                {
                    return lastIndex + 1;
                }
            }

            int ret;
            if (bNewWay && reader.ReadBit())
            {
                ret = (int)reader.ReadInt(3);  // read 3 bits
            }
            else
            {
                ret = (int)reader.ReadInt(7); // read 7 bits
                switch (ret & (32 | 64))
                {
                    case 32:
                        ret = (ret & ~96) | ((int)reader.ReadInt(2) << 5);
                        break;
                    case 64:
                        ret = (ret & ~96) | ((int)reader.ReadInt(4) << 5);
                        break;
                    case 96:
                        ret = (ret & ~96) | ((int)reader.ReadInt(7) << 5);
                        break;
                }
            }

            if (ret == 0xFFF)
            { // end marker is 4095 for cs:go
                return -1;
            }

            return lastIndex + 1 + ret;
        }

        public void Leave()
        {
            foreach (var prop in Props)
                prop.Destroy();
        }

        public override string ToString()
        {
            return ID + ": " + this.ServerClass;
        }
    }

    class PropertyEntry
    {
        public readonly int Index;
        public FlattenedPropEntry Entry { get; private set; }

        public event EventHandler<PropertyUpdateEventArgs<int>> IntRecived;
        public event EventHandler<PropertyUpdateEventArgs<long>> Int64Received;
        public event EventHandler<PropertyUpdateEventArgs<float>> FloatRecived;
        public event EventHandler<PropertyUpdateEventArgs<Vector>> VectorRecived;
        public event EventHandler<PropertyUpdateEventArgs<string>> StringRecived;
        public event EventHandler<PropertyUpdateEventArgs<object[]>> ArrayRecived;

        public void Decode(IBitStream stream, Entity e)
        {
            //I found no better place for this, sorry.
            //This checks, when in Debug-Mode
            //whether you've bound to the right event
            //Helps finding bugs, where you'd simply miss an update
            CheckBindings(e);

            //So here you start decoding. If you really want
            //to implement this yourself, GOOD LUCK.
            //also, be warned: They have 11 ways to read floats.
            //oh, btw: You may want to read the original Valve-code for this.
            switch (Entry.Prop.Type)
            {
                case SendPropertyType.Int:
                    {
                        int val = PropDecoder.DecodeInt(Entry.Prop, stream);
                        IntRecived?.Invoke(this, new PropertyUpdateEventArgs<int>(val, e, this));
                    }
                    break;
                case SendPropertyType.Int64:
                    {
                        long val = PropDecoder.DecodeInt64(Entry.Prop, stream);
                        Int64Received?.Invoke(this, new PropertyUpdateEventArgs<long>(val, e, this));
                    }
                    break;
                case SendPropertyType.Float:
                    {
                        float val = PropDecoder.DecodeFloat(Entry.Prop, stream);
                        FloatRecived?.Invoke(this, new PropertyUpdateEventArgs<float>(val, e, this));
                    }
                    break;
                case SendPropertyType.Vector:
                    {
                        Vector val = PropDecoder.DecodeVector(Entry.Prop, stream);
                        VectorRecived?.Invoke(this, new PropertyUpdateEventArgs<Vector>(val, e, this));
                    }
                    break;
                case SendPropertyType.Array:
                    {
                        object[] val = PropDecoder.DecodeArray(Entry, stream);
                        ArrayRecived?.Invoke(this, new PropertyUpdateEventArgs<object[]>(val, e, this));
                    }
                    break;
                case SendPropertyType.String:
                    {
                        string val = PropDecoder.DecodeString(Entry.Prop, stream);
                        StringRecived?.Invoke(this, new PropertyUpdateEventArgs<string>(val, e, this));
                    }
                    break;
                case SendPropertyType.VectorXY:
                    {
                        Vector val = PropDecoder.DecodeVectorXY(Entry.Prop, stream);
                        VectorRecived?.Invoke(this, new PropertyUpdateEventArgs<Vector>(val, e, this));
                    }
                    break;
                default:
                    throw new NotImplementedException("Could not read property.");
            }
        }

        public PropertyEntry(FlattenedPropEntry prop, int index)
        {
            this.Entry = new FlattenedPropEntry(prop.PropertyName, prop.Prop, prop.ArrayElementProp);
            this.Index = index;
        }

        public void Destroy()
        {
            this.IntRecived = null;
            this.Int64Received = null;
            this.FloatRecived = null;
            this.ArrayRecived = null;
            this.StringRecived = null;
            this.VectorRecived = null;
        }

        public override string ToString()
        {
            return string.Format("[PropertyEntry: Entry={0}]", Entry);
        }

        [Conditional("DEBUG")]
        public void CheckBindings(Entity e)
        {
            if (IntRecived != null && this.Entry.Prop.Type != SendPropertyType.Int)
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Int));

            if (Int64Received != null && this.Entry.Prop.Type != SendPropertyType.Int64)
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Int64));

            if (FloatRecived != null && this.Entry.Prop.Type != SendPropertyType.Float)
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Float));

            if (StringRecived != null && this.Entry.Prop.Type != SendPropertyType.String)
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.String));

            if (ArrayRecived != null && this.Entry.Prop.Type != SendPropertyType.Array)
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Array));

            if (VectorRecived != null && (this.Entry.Prop.Type != SendPropertyType.Vector && this.Entry.Prop.Type != SendPropertyType.VectorXY))
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Vector));
        }

        public static void Emit(Entity entity, object[] captured)
        {
            foreach (object arg in captured)
            {
                if (arg is RecordedPropertyUpdate<int> intReceived)
                {
                    entity.Props[intReceived.PropIndex].IntRecived?.Invoke(null, new PropertyUpdateEventArgs<int>(intReceived.Value, entity, entity.Props[intReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<long> int64Received)
                {
                    entity.Props[int64Received.PropIndex].Int64Received?.Invoke(null, new PropertyUpdateEventArgs<long>(int64Received.Value, entity, entity.Props[int64Received.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<float> floatReceived)
                {
                    entity.Props[floatReceived.PropIndex].FloatRecived?.Invoke(null, new PropertyUpdateEventArgs<float>(floatReceived.Value, entity, entity.Props[floatReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<Vector> vectorReceived)
                {
                    entity.Props[vectorReceived.PropIndex].VectorRecived?.Invoke(null, new PropertyUpdateEventArgs<Vector>(vectorReceived.Value, entity, entity.Props[vectorReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<string> stringReceived)
                {
                    entity.Props[stringReceived.PropIndex].StringRecived?.Invoke(null, new PropertyUpdateEventArgs<string>(stringReceived.Value, entity, entity.Props[stringReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<object[]> arrayReceived)
                {
                    entity.Props[arrayReceived.PropIndex].ArrayRecived?.Invoke(null, new PropertyUpdateEventArgs<object[]>(arrayReceived.Value, entity, entity.Props[arrayReceived.PropIndex]));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    #region Update-Types
    class PropertyUpdateEventArgs<T> : EventArgs
    {
        public T Value { get; private set; }

        public Entity Entity { get; private set; }

        public PropertyEntry Property { get; private set; }

        public PropertyUpdateEventArgs(T value, Entity e, PropertyEntry p)
        {
            this.Value = value;
            this.Entity = e;
            this.Property = p;
        }
    }

    public class RecordedPropertyUpdate<T>
    {
        public int PropIndex;
        public T Value;

        public RecordedPropertyUpdate(int propIndex, T value)
        {
            PropIndex = propIndex;
            Value = value;
        }
    }
    #endregion
}
