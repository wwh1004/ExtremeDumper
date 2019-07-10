namespace MetadataLocator {
	/// <summary>
	/// Metadata stream info
	/// </summary>
	internal sealed unsafe class MetadataStreamInfo {
		private readonly void* _address;
		private readonly uint _length;

		/// <summary>
		/// Address of stream
		/// </summary>
		public void* Address => _address;

		/// <summary>
		/// Length of stream
		/// </summary>
		public uint Length => _length;

		internal MetadataStreamInfo(void* address, uint length) {
			_address = address;
			_length = length;
		}
	}
}
