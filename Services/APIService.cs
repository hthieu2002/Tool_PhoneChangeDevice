using POCO.Models;

namespace Services
{
    public class APIService
    {
        public static int getRentCodeID(string deviceId)
        {
            var rentRaw = ADBService.curl(URL_Data.RENT_CODE_ID_REQUEST, deviceId);
            var rentObject = JsonService<RentCodeModel>.textToJsonObject(rentRaw);
            return rentObject.id;
        }
        public static RentCodeModel getRentCodeDataByID(int id, string deviceId)
        {
            var rentMsg = ADBService.curl(string.Format(URL_Data.RENT_CODE_CHECK_MESSAGE, id), deviceId);
            return JsonService<RentCodeModel>.textToJsonObject(rentMsg);
        }

        public static string getRentCodePhoneNumber(int id, string deviceId)
        {
            //var phoneNumber = "";
            //do
            //{
            //    Thread.Sleep(2000);
            //    var rentData = getRentCodeDataByID(id, deviceId);
            //    phoneNumber = rentData.phoneNumber;
            //} while (string.IsNullOrEmpty(phoneNumber));
            var rentData = getRentCodeDataByID(id, deviceId);
            return rentData.phoneNumber;
        }
        public static string getRentCodeMessage(int id, string deviceId)
        {
            //var message = "";
            //do
            //{
            //    Thread.Sleep(2000);
            //    var rentData = getRentCodeDataByID(id, deviceId);
            //    message = rentData.message;
            //} while (string.IsNullOrEmpty(message));
            var rentData = getRentCodeDataByID(id, deviceId);
            return rentData.message;
        }
    }
}
