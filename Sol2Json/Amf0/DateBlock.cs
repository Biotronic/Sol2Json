using System;

namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.Date)]
    public class DateBlock : Amf0Block<DateTime>
    {
        public DateBlock()
        {
        }

        public DateBlock(DateTime value) : base(value)
        {
        }
    }
}