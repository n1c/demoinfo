using DemoInfo.DT;
using System;
using System.Collections.Generic;

namespace DemoInfo.DP.Handler
{
    public static class PacketEntitesHandler
    {
        /// <summary>
        /// Decodes the bytes in the packet-entites message.
        /// </summary>
        /// <param name="packetEntities">Packet entities.</param>
        /// <param name="reader">Reader.</param>
        /// <param name="parser">Parser.</param>
        public static void Apply(PacketEntities packetEntities, IBitStream reader, DemoParser parser)
        {
            int currentEntity = -1;

            for (int i = 0; i < packetEntities.UpdatedEntries; i++)
            {

                //First read which entity is updated
                currentEntity += 1 + (int)reader.ReadUBitInt();

                //Find out whether we should create, destroy or update it.
                // Leave flag
                if (!reader.ReadBit())
                {
                    // enter flag
                    if (reader.ReadBit())
                    {
                        //create it
                        Entity e = ReadEnterPVS(reader, currentEntity, parser);

                        parser.Entities[currentEntity] = e;

                        e.ApplyUpdate(reader);
                    }
                    else
                    {
                        // preserve / update
                        Entity e = parser.Entities[currentEntity];
                        e.ApplyUpdate(reader);
                    }
                }
                else
                {
                    Entity e = parser.Entities[currentEntity];
                    if (e != null) // why is it sometimes null?
                    {
                        e.ServerClass.AnnounceDestroyedEntity(e);
                        e.Leave();
                        parser.Entities[currentEntity] = null;
                    }

                    // Dunno, but you gotta read this.
                    if (reader.ReadBit())
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Reads an update that occures when a new edict enters the PVS (potentially visible system)
        /// </summary>
        /// <returns>The new Entity.</returns>
        private static Entity ReadEnterPVS(IBitStream reader, int id, DemoParser parser)
        {
            //What kind of entity?
            int serverClassID = (int)reader.ReadInt(parser.SendTableParser.ClassBits);

            //So find the correct server class
            ServerClass entityClass = parser.SendTableParser.ServerClasses[serverClassID];

            _ = reader.ReadInt(10); //Entity serial.

            Entity newEntity = new Entity(id, entityClass);

            //give people the chance to subscribe to events for this
            newEntity.ServerClass.AnnounceNewEntity(newEntity);

            //And then parse the instancebaseline.
            //basically you could call
            //newEntity.ApplyUpdate(parser.instanceBaseline[entityClass];
            //This code below is just faster, since it only parses stuff once
            //which is faster.

            if (parser.PreprocessedBaselines.TryGetValue(serverClassID, out object[] fastBaseline))
            {
                PropertyEntry.Emit(newEntity, fastBaseline);
            }
            else
            {
                List<object> preprocessedBaseline = new List<object>();

                if (parser.InstanceBaseline.ContainsKey(serverClassID))
                {
                    // @TODO: I don't understand this first using? Does `collector` get used?
                    using (PropertyCollector collector = new PropertyCollector(newEntity, preprocessedBaseline))
                    using (IBitStream bitStream = BitStreamUtil.Create(parser.InstanceBaseline[serverClassID]))
                    {
                        newEntity.ApplyUpdate(bitStream);
                    }
                }

                parser.PreprocessedBaselines.Add(serverClassID, preprocessedBaseline.ToArray());
            }

            return newEntity;
        }

        private class PropertyCollector : IDisposable
        {
            private readonly Entity Underlying;
            private readonly IList<object> Capture;

            public PropertyCollector(Entity underlying, IList<object> capture)
            {
                Underlying = underlying;
                Capture = capture;

                foreach (PropertyEntry prop in Underlying.Props)
                {
                    switch (prop.Entry.Prop.Type)
                    {
                        case SendPropertyType.Array:
                            prop.ArrayRecieved += HandleArrayRecieved;
                            break;
                        case SendPropertyType.Float:
                            prop.FloatRecieved += HandleFloatRecieved;
                            break;
                        case SendPropertyType.Int:
                            prop.IntReceived += HandleIntRecieved;
                            break;
                        case SendPropertyType.Int64:
                            prop.Int64Received += HandleInt64Received;
                            break;
                        case SendPropertyType.String:
                            prop.StringRecieved += HandleStringRecieved;
                            break;
                        case SendPropertyType.Vector:
                        case SendPropertyType.VectorXY:
                            prop.VectorRecieved += HandleVectorRecieved;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            private void HandleVectorRecieved(object sender, PropertyUpdateEventArgs<Vector> e) { Capture.Add(e.Record()); }
            private void HandleStringRecieved(object sender, PropertyUpdateEventArgs<string> e) { Capture.Add(e.Record()); }
            private void HandleIntRecieved(object sender, PropertyUpdateEventArgs<int> e) { Capture.Add(e.Record()); }
            private void HandleInt64Received(object sender, PropertyUpdateEventArgs<long> e) { Capture.Add(e.Record()); }
            private void HandleFloatRecieved(object sender, PropertyUpdateEventArgs<float> e) { Capture.Add(e.Record()); }
            private void HandleArrayRecieved(object sender, PropertyUpdateEventArgs<object[]> e) { Capture.Add(e.Record()); }

            public void Dispose()
            {
                foreach (PropertyEntry prop in Underlying.Props)
                {
                    switch (prop.Entry.Prop.Type)
                    {
                        case SendPropertyType.Array:
                            prop.ArrayRecieved -= HandleArrayRecieved;
                            break;
                        case SendPropertyType.Float:
                            prop.FloatRecieved -= HandleFloatRecieved;
                            break;
                        case SendPropertyType.Int:
                            prop.IntReceived -= HandleIntRecieved;
                            break;
                        case SendPropertyType.Int64:
                            prop.Int64Received -= HandleInt64Received;
                            break;
                        case SendPropertyType.String:
                            prop.StringRecieved -= HandleStringRecieved;
                            break;
                        case SendPropertyType.Vector:
                        case SendPropertyType.VectorXY:
                            prop.VectorRecieved -= HandleVectorRecieved;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
