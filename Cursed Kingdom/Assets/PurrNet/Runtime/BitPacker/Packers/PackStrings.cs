using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class PackStrings
    {
        [UsedByIL]
        public static void Write(this BitPacker packer, string value)
        {
            if (value == null)
            {
                packer.Write(false);
                return;
            }
            
            packer.Write(true);
            
            int strLen = value.Length;
            
            packer.Write(strLen);
            
            for (int i = 0; i < strLen; i++)
                packer.Write(value[i]);
        }
        
        [UsedByIL]
        public static void Read(this BitPacker packer, ref string value)
        {
            bool hasValue = false;
            
            packer.Read(ref hasValue);
            
            if (!hasValue)
            {
                value = null;
                return;
            }
            
            int strLen = 0;
            
            packer.Read(ref strLen);
            
            var chars = new char[strLen];

            for (int i = 0; i < strLen; i++)
            {
                packer.Read(ref chars[i]);
            }
            
            value = new string(chars);
        }
        
        [UsedByIL]
        public static void Write(this BitPacker packer, char value)
        {
            packer.WriteBits(value, 8);
        }
        
        [UsedByIL]
        public static void Read(this BitPacker packer, ref char value)
        {
            value = (char)packer.ReadBits(8);
        }
    }
}
