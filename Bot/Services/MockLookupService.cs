using System;

namespace Bot.Services
{
    /// <summary>
    /// A mock lookup service - doesn't hit any backend
    /// </summary>
    /// <seealso cref="Bot.Services.IFineLookupService" />
    public class MockLookupService : IFineLookupService
    {
        /// <summary>
        /// Looks up details of a Fine
        /// </summary>
        /// <param name="noticeOrVehicleNumber">The notice or vehicle number of the target fine.</param>
        /// <returns>
        /// A <see cref="FineDetails" /> instance describing the fine.
        /// </returns>
        /// <exception cref="ArgumentException">No notice or vehicle # was found matching {noticeOrVehicleNumber}.</exception>
        public FineDetails LookUpFine(string noticeOrVehicleNumber)
        {
            if (noticeOrVehicleNumber.StartsWith("z", StringComparison.OrdinalIgnoreCase))
            {   // so we can test failures
                throw new ArgumentException($"No notice or vehicle # was found matching {noticeOrVehicleNumber}. Please check the number and try again.", nameof(noticeOrVehicleNumber));
            }
            else
            {
                return new FineDetails(noticeOrVehicleNumber, Math.Round(new Random().NextDouble() * 100, 2), GetRandomDescription());
            }
        }

        private string GetRandomDescription() => LoremNET.Lorem.Sentence(new Random().Next(5, 20));
    }
}