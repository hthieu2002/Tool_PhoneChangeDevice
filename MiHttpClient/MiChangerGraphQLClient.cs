using POCO.Models;
using POCO.ResponseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiHttpClient
{
    public class MiChangerGraphQLClient : MiGraphQLClientBase
    {
        public MiChangerGraphQLClient(string endpoint, string authenticationType, string authenticationValue)
            : base(endpoint, authenticationType, authenticationValue)
        { }

        public async Task<MutationResponseModel> ReleaseProxy(string serialNo, string proxyId)
        {
            var query =
                @"mutation releaseProxy($serialNo: String, $proxyId: String!) {
                        releaseProxy(serialNo: $serialNo, proxyId: $proxyId) {
                            result
                            message
                    }
                }
                ";
            try
            {
                var response = await SendQueryAsync<ReleaseProxyModel>(query, new
                {
                    serialNo = serialNo,
                    proxyId = proxyId
                });
                return response.ReleaseProxy;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<string> GetGoogleDriveAccessToken()
        {
            var query =
                @"query getGoogleDriveAccessToken {
                        getGoogleDriveAccessToken {
                            access_token
                        }
                }";
            try
            {
                var response = await SendQueryAsync<GetGoogleDriveAccessTokenResponse>(query, new { });
                return response.GetGoogleDriveAccessToken.Access_token;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<SingleProxySummary>> GetProxySummary()
        {
            var query =
                @"query getProxySummary {
                        getProxySummary {
                            data {
                                  type
                                  quantity
                                }
                        }
                }";
            try
            {
                var response = await SendQueryAsync<GetProxySummaryResponse>(query, new { });
                if (response.GetProxySummary == null)
                    throw new Exception("NULL");
                return response.GetProxySummary.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ProxyModel> GetProxy(string serialNo)
        {
            var query =
                @"query getProxy($serialNo: String!) {
                        getProxy(serialNo: $serialNo) {
                                id
                                proxy
                                type
                                message
                                apiKey
                                user    
                                password
                                resetWebHook
                                protocol
                            
                    }
                }
                ";
            try
            {
                var response = await SendQueryAsync<GetProxyResponse>(query, new
                {
                    serialNo = serialNo,
                });
                if (response.GetProxy == null)
                    throw new Exception("NULL");
                return response.GetProxy;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<LicensesModel> GetActiveLicenses()
        {
            var query =
                @"query getActiveLicenses {
                        getActiveLicenses {
                            licenses {
                                serialNo
                                durationInDay
                                email
                                type
                                dateEnd
                                dateStart
                                model
                            }
                    }
                }
                ";
            try
            {
                var response = await SendQueryAsync<GetActiveLicensesResponse>(query, new { });
                if (response.GetActiveLicenses == null)
                    throw new Exception("NULL");
                return response.GetActiveLicenses;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<LicensesModel> GetActiveLicensesBySerialNo(string serialNo = "")
        {
            var query =
                @"query getActiveLicensesBySerialNo($serialNo: String!) {
                        getActiveLicensesBySerialNo(serialNo: $serialNo) {
                            licenses {
                                serialNo
                                durationInDay
                                email
                                type
                                model
                            }
                    }
                }
                ";
            try
            {
                var response = await SendQueryAsync<GetActiveLicensesBySerialNoResponse>(query, new
                {
                    serialNo = serialNo,
                });
                if (response.GetActiveLicensesBySerialNo == null)
                    throw new Exception("NULL");
                return response.GetActiveLicensesBySerialNo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private DeviceModel NormalizeDeviceResponse(DeviceModel model)
        { 
            string[] fingerPrintString = model.Fingerprint.Split('/');
            model.Product = fingerPrintString[1];
            var indexComma = fingerPrintString[2].IndexOf(':');
            model.Code = fingerPrintString[2].Substring(0, indexComma);
            model.Release = fingerPrintString[2].Substring(indexComma + 1);
            switch (model.Release)
            {
                case "7.0":
                    model.SDK = "24";
                    break;
                case "8.0.0":
                    model.SDK = "26";
                    break;
                case "8.1.0":
                    model.SDK = "27";
                    break;
                case "9":
                    model.SDK = "28";
                    break;
                case "10":
                    model.SDK = "29";
                    break;
                case "11":
                    model.SDK = "30";
                    break;
                default:
                    model.SDK = "25";
                    break;
            }
            model.BuildId = fingerPrintString[3];
            model.BuildIncremental = fingerPrintString[4].Replace(":user", "");
            if (string.IsNullOrEmpty(model.Bootloader))
                model.Bootloader = model.BuildIncremental;
            model.BuildFlavor = string.Concat(model.Product, "-user");
            if (string.IsNullOrEmpty(model.BuildDisplayId))
                model.BuildDisplayId = string.Format("{0}.{1}", model.BuildId, model.BuildIncremental);
            model.BuildDate = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(model.BuildDateUtc + "000")).ToString("ddd MMM dd HH:mm:ss 'UTC' yyyy");
            model.BuildDescription = string.Format("{0} {1} {2} {3} release-keys"
                                , model.Product
                                , model.Release
                                , model.BuildId
                                , model.BuildIncremental);                ////[crownltexx-user 10 QP1A.190711.020 N960FXXU6ETG3 release-keys]
            //if (string.IsNullOrEmpty(model.Platform))
            //{
            //    model.Platform = "msm8953";
            //}
            return model;
        }
        public async Task<DeviceModel> GetRandomDeviceV3(
            string brand = "",
            string model = "",
            int sdkMin = 24,
            int sdkMax = 32)
        {
            var query = @"
                     query getDeviceV3($brand: String, $model: String, $sdkMin: Int, $sdkMax: Int) {
                        getDeviceV3(brand: $brand, model: $model, sdkMin: $sdkMin, sdkMax: $sdkMax) {
							  model
                              gaid
                              board
                              baseband
                              securityPath
                              name
                              fingerprint
                              buildDisplayId
                              manufacturer
                              buildDateUtc
                              hardware
                              imei
                              imei1
                              buildHost
                              gsf
                              platform
                              bootloader
                              #index
                        }
                    }
                ";
            try
            {
                var response = await SendQueryAsync<GetDeviceV3Response>(query, new
                {
                    brand = brand,
                    model = model,
                    sdkMin = sdkMin,
                    sdkMax = sdkMax
                });
                if (response.GetDeviceV3 == null)
                    throw new Exception("NULL");
                var result = response.GetDeviceV3;
                return NormalizeDeviceResponse(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<DeviceModel> GetRandomDeviceNew()
        {
            var query = @"
                     query getDeviceWadogeNew {
                        getDeviceWadogeNew {
							  model
                              gaid
                              board
                              baseband
                              securityPath
                              name
                              fingerprint
                              buildDisplayId
                              manufacturer
                              buildDateUtc
                              hardware
                              imei
                              imei1
                              buildHost
                              gsf
                              platform
                              bootloader
                        }
                    }
                ";
            try
            {
                var response = await SendQueryAsync<GetDeviceWadogeNewResponse>(query, new
                {
                });
                if (response.GetDeviceWadogeNew == null)
                    throw new Exception("NULL");
                var result = response.GetDeviceWadogeNew;
                return NormalizeDeviceResponse(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<DeviceModel> GetRandomDevice()
        {
            var query = @"
                     query getDeviceWadoge {
                        getDeviceWadoge {
							  model
                              gaid
                              board
                              baseband
                              securityPath
                              name
                              fingerprint
                              buildDisplayId
                              manufacturer
                              buildDateUtc
                              hardware
                              imei
                              imei1
                              buildHost
                              gsf
                              bootloader
                        }
                    }
                ";
            try
            {
                var response = await SendQueryAsync<GetDeviceWadogeResponse>(query, new
                {
                });
                if (response.GetDeviceWadoge == null)
                    throw new Exception("NULL");
                var result = response.GetDeviceWadoge;
                return NormalizeDeviceResponse(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<DeviceModel> GetRandomDevice(
            string email = "",
            string imei = "",
            string serialNo = "",
            string packages = "",
            string countryIso = "",
            string carrierName = "",
            string model = "TISSOT")
        {
            var query = @"
                     query getDevice($email: String, $imei: String, $serialNo: String!, $packages: String, $simInfo: SimInfo, $model: ModelENUM) {
                        getDevice (email: $email, imei: $imei, serialNo: $serialNo, packages: $packages, simInfo: $simInfo, model: $model){
							serialNo 
							model
							board
							hardware
							manufacturer
							brand
							product
							platform
							imei
                            imei1
							code
							wifiMacAddress
							bluetoothMacAddress
							androidId
							#Build Info
							fingerprint
							release
							sdk
							securityPath
							buildHost
							buildId
							buildDisplayId
							buildIncremental
							buildDescription
							buildDate
							buildDateUtc
							buildFlavor
							#GPS info
							lon
							lat
							#Sim info
							iccid
							imsi
							simPhoneNumber
							simOperatorNumeric
							simOperatorCountry
							simOperatorName
							gaid
							gsf
                        }
                    }
                ";
            try
            {
                var response = await SendQueryAsync<GetDeviceResponse>(query, new
                {
                    email = email,
                    imei = imei,
                    serialNo = serialNo,
                    packages = packages,
                    simInfo = new
                    {
                        countryIso = countryIso,
                        carrierName = carrierName
                    },
                    model = model
                });
                if (response.GetDevice == null)
                    throw new Exception("NULL");
                var result = response.GetDevice;
                if (string.IsNullOrEmpty(result.Bootloader))
                    result.Bootloader = result.BuildIncremental;
                return response.GetDevice;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
    }
}
