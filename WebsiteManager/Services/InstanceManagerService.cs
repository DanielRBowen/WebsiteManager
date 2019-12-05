using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebsiteManager.Models;

namespace WebsiteManager.Services
{
    public class InstanceManagerService : IInstanceManagerService
    {
        private readonly ILogger<InstanceManagerService> _logger;
        private readonly IConfiguration _config;

        public InstanceManagerService(ILogger<InstanceManagerService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<bool> TryCreateNewInstance(InstanceConfiguration instanceConfiguration)
        {
            try
            {
                var instanceName = instanceConfiguration.Name;

                if (DoesDatabaseExist(instanceName))
                {
                    _logger.LogError("The database for this instance already exists. Database Name: {instanceName}", instanceName);
                    return false;
                }

                _logger.LogInformation("Instance is being created. Instance Name: {instanceName}", instanceName);
                var directoryPath = Directory.GetCurrentDirectory();
                string originalSourceFolderName = _config.GetValue<string>("OriginalSourceFolderName");
                string sourcePath = $"{directoryPath}\\..\\{originalSourceFolderName}";
                string destinationPath = $"{directoryPath}\\..\\{originalSourceFolderName}_{instanceName}";

                if (Directory.Exists(sourcePath) == false)
                {
                    _logger.LogError($"Original Source path doesn't exist. Source Path: {sourcePath}");
                    return false;
                }

                //DirectoryUtility.DirectoryCopy(sourcePath, destinationPath, true);
                //string copyOutput = null;
                CommandlineUtility.Copy(sourcePath, destinationPath, null);

                //if (string.IsNullOrWhiteSpace(copyOutput))
                //{
                //    _logger.LogError("Couldn't copy correctly. The copy output is null");
                //    return false;
                //    throw new InvalidOperationException("Unable to publish using cli");
                //}
                //else
                //{
                //}
                await Task.Delay(5000);
                _logger.LogInformation("Template source copied from: {sourcePath} to: {destinationPath}", sourcePath, destinationPath);
                string appSettingsPath = $"{destinationPath}\\{originalSourceFolderName}\\appsettings.json";
                bool isNewDatabaseNameWritten = false;

                if (File.Exists(appSettingsPath))
                {
                    isNewDatabaseNameWritten = await TryWriteNewDatabaseName(instanceName, appSettingsPath);
                }
                else
                {
                    _logger.LogError($"Appsettings path doesn't exist at this time. Appsettings path Path: {appSettingsPath}");
                    return false;
                }

                //string publishOutput = null;

                if (isNewDatabaseNameWritten == false)
                {
                    _logger.LogError($"The database name could not be written.");
                    return false;
                }

                _logger.LogInformation($"New Database name written to appsettings. Path: {appSettingsPath}");
                string layoutPath = $"{destinationPath}\\{originalSourceFolderName}\\Views\\Shared\\_layout.cshtml";
                bool isWebsiteNameWritten = TryWriteWebsiteName(layoutPath, instanceName, originalSourceFolderName);

                if (isWebsiteNameWritten == false)
                {
                    _logger.LogError($"The Website name could not be written to the template.");
                    return false;
                }

                CommandlineUtility.BuildAndPublish(destinationPath);
                _logger.LogInformation($"New instance built and published with name: {instanceName}");

                //publishOutput = await CommandlineUtility.BuildAndPublish(destinationPath);

                //if (string.IsNullOrWhiteSpace(publishOutput))
                //{
                //    _logger.LogError("Couldn't publish correctly. The publish output is null");
                //    return false;
                //    throw new InvalidOperationException("Unable to publish using cli");
                //}
                //else
                //{

                //}


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        private bool TryWriteWebsiteName(string layoutPath, string instanceName, string originalSourceFolderName)
        {
            try
            {
                string layout = File.ReadAllText(layoutPath);
                layout = layout.Replace(originalSourceFolderName, instanceName);
                File.WriteAllText(layoutPath, layout);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public bool TryDeleteInstance(InstanceConfiguration instanceConfiguration)
        {
            var instanceName = instanceConfiguration.Name;
            _logger.LogInformation("Instance is being Deleted. Instance Name: {Name}", instanceName);
            var directoryPath = Directory.GetCurrentDirectory();
            string originalSourceFolderName = _config.GetValue<string>("OriginalSourceFolderName");
            string instancePath = $"{directoryPath}\\..\\{originalSourceFolderName}_{instanceName}";

            if (TryDeleteInstanceFolder(instancePath) == false)
            {
                return false;
            }

            if (DoesDatabaseExist(instanceName))
            {
                using var sqlConnection = new SqlConnection($"Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true");
                sqlConnection.Open();
                var sqlCommandText = @"ALTER DATABASE " + instanceName + @" SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [" + instanceName + "]";
                var sqlCommand = new SqlCommand(sqlCommandText, sqlConnection);
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }

            _logger.LogInformation("Instance was successfully Deleted. Instance Name: {Name}", instanceName);
            return true;
        }

        private bool TryDeleteInstanceFolder(string instancePath)
        {
            if (Directory.Exists(instancePath) == false)
            {
                _logger.LogError("The instance could not be deleted becasue it could not be found. Instance Path: {instancePath}", instancePath);
                return false;
            }

            try
            {
                Directory.Delete(instancePath, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

            Thread.Sleep(1000);

            if (Directory.Exists(instancePath))
            {
                _logger.LogError("The instance was not deleted. Instance Path: {instancePath}", instancePath);
                return false;
            }

            return true;
        }

        //https://www.newtonsoft.com/json/help/html/ModifyJson.htm
        private async Task<bool> TryWriteNewDatabaseName(string instanceName, string appsettingsPath)
        {
            try
            {
                _logger.LogInformation($"Writing New Database Name to appsettings. Database Name: {instanceName}");
                var jsonString = await File.ReadAllTextAsync(appsettingsPath);
                var jObject = JObject.Parse(jsonString);
                jObject["ConnectionStrings"]["DefaultConnection"] = $"Server=(localdb)\\mssqllocaldb;Database={instanceName};Trusted_Connection=True;MultipleActiveResultSets=true";
                File.WriteAllText(appsettingsPath, jObject.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        //https://stackoverflow.com/questions/2232227/check-if-database-exists-before-creating
        private bool DoesDatabaseExist(string databaseName)
        {
            bool result = false;

            try
            {
                var sqlCreateDBQuery = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName);
                using var sqlConnection = new SqlConnection($"Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true");
                using var sqlCommand = new SqlCommand(sqlCreateDBQuery, sqlConnection);
                sqlConnection.Open();
                object resultObj = sqlCommand.ExecuteScalar();
                int databaseID = 0;

                if (resultObj != null)
                {
                    int.TryParse(resultObj.ToString(), out databaseID);
                }

                sqlConnection.Close();
                result = (databaseID > 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                result = false;
            }

            return result;
        }
    }
}
