namespace Nextension
{
    public interface INByteable
    {
        public byte[] getBytes();
        public void setBytes(byte[] inData, ref int startIndex);
    }
}
