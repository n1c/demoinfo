using DemoInfo.DP.Handler;
using DemoInfo.DT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DemoInfo.DP
{
    internal class Entity
    {
        public int ID { get; set; }

        public ServerClass ServerClass { get; set; }

        public PropertyEntry[] Props { get; private set; }

        public event EventHandler<EntityLeftPVSEventArgs> EntityLeft;

        public Entity(int id, ServerClass serverClass)
        {
            ID = id;
            ServerClass = serverClass;

            List<FlattenedPropEntry> flattenedProps = ServerClass.FlattenedProps;
            Props = new PropertyEntry[flattenedProps.Count];
            for (int i = 0; i < flattenedProps.Count; i++)
            {
                Props[i] = new PropertyEntry(flattenedProps[i], i);
            }
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
            //First a list of the updated props is sent
            //And then the props itself are sent.

            //Read the field-indicies in a "new" way?
            bool newWay = reader.ReadBit();
            int index = -1;
            List<PropertyEntry> entries = new List<PropertyEntry>();

            //No read them.
            while ((index = ReadFieldIndex(reader, index, newWay)) != -1)
            {
                entries.Add(Props[index]);
            }

            //Now read the updated props
            foreach (PropertyEntry prop in entries)
            {
                prop.Decode(reader, this);
            }
        }

        private int ReadFieldIndex(IBitStream reader, int lastIndex, bool bNewWay)
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

            return ret == 0xFFF ? -1 : lastIndex + 1 + ret;
        }

        public void Leave()
        {
            EntityLeft?.Invoke(this, new EntityLeftPVSEventArgs(this));

            foreach (PropertyEntry prop in Props)
            {
                prop.Destroy();
            }
        }

        public override string ToString()
        {
            return ID + ": " + ServerClass;
        }
    }

    internal class PropertyEntry
    {
        public readonly int Index;
        public FlattenedPropEntry Entry { get; private set; }

        public event EventHandler<PropertyUpdateEventArgs<int>> IntReceived;
        public event EventHandler<PropertyUpdateEventArgs<long>> Int64Received;
        public event EventHandler<PropertyUpdateEventArgs<float>> FloatRecieved;
        public event EventHandler<PropertyUpdateEventArgs<Vector>> VectorRecieved;
        public event EventHandler<PropertyUpdateEventArgs<string>> StringRecieved;
        public event EventHandler<PropertyUpdateEventArgs<object[]>> ArrayRecieved;

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
                        IntReceived?.Invoke(this, new PropertyUpdateEventArgs<int>(val, e, this));
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
                        FloatRecieved?.Invoke(this, new PropertyUpdateEventArgs<float>(val, e, this));
                    }
                    break;
                case SendPropertyType.Vector:
                    {
                        Vector val = PropDecoder.DecodeVector(Entry.Prop, stream);
                        VectorRecieved?.Invoke(this, new PropertyUpdateEventArgs<Vector>(val, e, this));
                    }
                    break;
                case SendPropertyType.Array:
                    {
                        object[] val = PropDecoder.DecodeArray(Entry, stream);
                        ArrayRecieved?.Invoke(this, new PropertyUpdateEventArgs<object[]>(val, e, this));
                    }
                    break;
                case SendPropertyType.String:
                    {
                        string val = PropDecoder.DecodeString(Entry.Prop, stream);
                        StringRecieved?.Invoke(this, new PropertyUpdateEventArgs<string>(val, e, this));
                    }
                    break;
                case SendPropertyType.VectorXY:
                    {
                        Vector val = PropDecoder.DecodeVectorXY(Entry.Prop, stream);
                        VectorRecieved?.Invoke(this, new PropertyUpdateEventArgs<Vector>(val, e, this));
                    }
                    break;
                default:
                    throw new NotImplementedException("Could not read property.");
            }
        }

        public PropertyEntry(FlattenedPropEntry prop, int index)
        {
            Entry = new FlattenedPropEntry(prop.PropertyName, prop.Prop, prop.ArrayElementProp);
            Index = index;
        }

        public void Destroy()
        {
            IntReceived = null;
            Int64Received = null;
            FloatRecieved = null;
            ArrayRecieved = null;
            StringRecieved = null;
            VectorRecieved = null;
        }

        public override string ToString()
        {
            return string.Format("[PropertyEntry: Entry={0}]", Entry);
        }

        [Conditional("DEBUG")]
        public void CheckBindings(Entity e)
        {
            if (IntReceived != null && Entry.Prop.Type != SendPropertyType.Int)
            {
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Int));
            }

            if (Int64Received != null && Entry.Prop.Type != SendPropertyType.Int64)
            {
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Int64));
            }

            if (FloatRecieved != null && Entry.Prop.Type != SendPropertyType.Float)
            {
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Float));
            }

            if (StringRecieved != null && Entry.Prop.Type != SendPropertyType.String)
            {
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.String));
            }

            if (ArrayRecieved != null && Entry.Prop.Type != SendPropertyType.Array)
            {
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Array));
            }

            if (VectorRecieved != null
                && Entry.Prop.Type != SendPropertyType.Vector
                && Entry.Prop.Type != SendPropertyType.VectorXY)
            {
                throw new InvalidOperationException(
                    string.Format("({0}).({1}) isn't an {2}",
                        e.ServerClass.Name,
                        Entry.PropertyName,
                        SendPropertyType.Vector));
            }
        }

        public static void Emit(Entity entity, object[] captured)
        {
            foreach (object arg in captured)
            {
                if (arg is RecordedPropertyUpdate<int> intReceived)
                {
                    entity.Props[intReceived.PropIndex].IntReceived?.Invoke(null, new PropertyUpdateEventArgs<int>(intReceived.Value, entity, entity.Props[intReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<long> int64Received)
                {
                    entity.Props[int64Received.PropIndex].Int64Received?.Invoke(null, new PropertyUpdateEventArgs<long>(int64Received.Value, entity, entity.Props[int64Received.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<float> floatReceived)
                {
                    entity.Props[floatReceived.PropIndex].FloatRecieved?.Invoke(null, new PropertyUpdateEventArgs<float>(floatReceived.Value, entity, entity.Props[floatReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<Vector> vectorReceived)
                {
                    entity.Props[vectorReceived.PropIndex].VectorRecieved?.Invoke(null, new PropertyUpdateEventArgs<Vector>(vectorReceived.Value, entity, entity.Props[vectorReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<string> stringReceived)
                {
                    entity.Props[stringReceived.PropIndex].StringRecieved?.Invoke(null, new PropertyUpdateEventArgs<string>(stringReceived.Value, entity, entity.Props[stringReceived.PropIndex]));
                }
                else if (arg is RecordedPropertyUpdate<object[]> arrayReceived)
                {
                    entity.Props[arrayReceived.PropIndex].ArrayRecieved?.Invoke(null, new PropertyUpdateEventArgs<object[]>(arrayReceived.Value, entity, entity.Props[arrayReceived.PropIndex]));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    #region Update-Types
    internal class PropertyUpdateEventArgs<T> : EventArgs
    {
        public T Value { get; private set; }

        public Entity Entity { get; private set; }

        public PropertyEntry Property { get; private set; }

        public PropertyUpdateEventArgs(T value, Entity e, PropertyEntry p)
        {
            Value = value;
            Entity = e;
            Property = p;
        }
    }

    internal class RecordedPropertyUpdate<T>
    {
        public int PropIndex;
        public T Value;

        public RecordedPropertyUpdate(int propIndex, T value)
        {
            PropIndex = propIndex;
            Value = value;
        }
    }

    internal class EntityLeftPVSEventArgs
    {
        public Entity Entity;

        public EntityLeftPVSEventArgs(Entity entity)
        {
            Entity = entity;
        }
    }
    #endregion
}
