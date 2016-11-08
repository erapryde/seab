using Newtonsoft.Json;

namespace Bot.Services
{
    /// <summary>
    /// Defines an interface for <see cref="FineDetails"/> returned from a Fine Lookup Service
    /// </summary>
    interface IFineLookupService
    {
        /// <summary>
        /// Looks up details of a Fine
        /// </summary>
        /// <param name="noticeOrVehicleNumber">The notice or vehicle number of the target fine.</param>
        /// <returns>A <see cref="FineDetails"/> instance describing the fine.</returns>
        /// <exception cref="System.ArgumentException">No fine was found with an Id matching <paramref name="noticeOrVehicleNumber"/></exception>
        FineDetails LookUpFine(string noticeOrVehicleNumber);
    }

    /// <summary>
    /// Holds the details of a fine
    /// </summary>
    public sealed class FineDetails
    {
        public FineDetails(string id, double amt, string desc = null)
        {
            this.Id = id;
            this.Description = desc;
            this.Amount = amt;
        }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("amount")]
        public double Amount { get; set; }
    }
}
