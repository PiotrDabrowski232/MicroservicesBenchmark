namespace SharedKernel.VariablesExtensions
{
    public static class IntExtensions
    {
        public static Guid ToGuid(this Int32 value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}
