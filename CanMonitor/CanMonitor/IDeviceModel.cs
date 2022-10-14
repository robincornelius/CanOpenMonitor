namespace PFMMeasurementService.Models.Devices
{
    /// <summary>
    /// IDeviceModel is used to represent required properties of any hardware device within the system.
    /// </summary>
    public interface IDeviceModel
    {
        /// <summary>
        /// Gets the URI base, used as part of the full URI of the device.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets a human friendly name for the device for UI purposes.
        /// MUST NOT be used as an identifier by code.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// simple Status reporting for the device
        /// </summary>
        bool isOk { get; }

        /// <summary>
        /// Initalise this peice of hardware
        /// </summary>
        void Init();

    }
}
