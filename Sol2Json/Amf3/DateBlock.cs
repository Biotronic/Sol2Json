using System;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Date)]
    public class DateBlock : Amf3Block<DateTime>
    {
        public DateBlock()
        {
        }

        public DateBlock(DateTime value) : base(value)
        {
        }
    }
}