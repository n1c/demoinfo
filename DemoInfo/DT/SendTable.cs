using System.Collections.Generic;

namespace DemoInfo.DT
{
    internal class SendTable
    {
        public List<SendTableProperty> Properties { get; } = new List<SendTableProperty>();
        public string Name { get; set; }
        public bool IsEnd { get; set; }

        public SendTable(IBitStream bitstream)
        {
            DemoInfo.SendTable dataTable = new DemoInfo.SendTable();

            foreach (DemoInfo.SendTable.SendProp prop in dataTable.Parse(bitstream))
            {
                SendTableProperty property = new SendTableProperty()
                {
                    DataTableName = prop.DtName,
                    HighValue = prop.HighValue,
                    LowValue = prop.LowValue,
                    Name = prop.VarName,
                    NumberOfBits = prop.NumBits,
                    NumberOfElements = prop.NumElements,
                    Priority = prop.Priority,
                    RawFlags = prop.Flags,
                    RawType = prop.Type
                };

                Properties.Add(property);
            }

            Name = dataTable.NetTableName;
            IsEnd = dataTable.IsEnd;
        }
    }
}

