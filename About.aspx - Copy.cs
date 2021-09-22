using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Program4NONSQL
{
    public partial class About : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public class CustomerEntity : TableEntity
        {
            public CustomerEntity()
            {
            }

            public CustomerEntity(string lastName, string firstName)
            {
                PartitionKey = lastName;
                RowKey = firstName;
            }

            public string information { get; set; }

        }

        public async Task UploadAsync()
        {
            
            WebRequest request = WebRequest.Create("https://s3-us-west-2.amazonaws.com/css490/input.txt");

            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();

            BlobContainerClient container = new BlobContainerClient(connectionString, "mcchowcontainer");

            try
            {            
            await container.CreateAsync();
            container.SetAccessPolicy(PublicAccessType.Blob);
            }
            catch(Exception ex)
            {

            }
            try
            {
                container.DeleteBlobIfExists("mcchowfile.txt");

                // Get a reference to a blob
                BlobClient blob = container.GetBlobClient("mcchowfile.txt");

                blob.Upload(dataStream);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task InsertOrMergeEntityAsync()
        {
            string rt;
            string[] datas;


            WebRequest request = WebRequest.Create("https://s3-us-west-2.amazonaws.com/css490/input.txt");
            //WebRequest request = WebRequest.Create("https://css436mcchowisall.blob.core.windows.net/mcchowcontainer/mcchowfiletest.txt");

            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();

            await UploadAsync();

            StreamReader reader = new StreamReader(dataStream);

            rt = reader.ReadToEnd();

            datas = rt.Split('\n');

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("css436mcchownametoinfo");

            foreach (string data in datas)
            {
                Console.WriteLine("Data: {0}", data);
                var firstWord = data.Substring(0, data.IndexOf(" "));
                string remain = data.Substring(data.IndexOf(" ") + 1);
                var secondWord = remain.Substring(0, remain.IndexOf(" "));

                remain = remain.Substring(remain.IndexOf(" ") + 1);

                Console.WriteLine("Firstname: {0}", firstWord);
                Console.WriteLine("Lastname: {0}", secondWord);
                Console.WriteLine(remain);

                CustomerEntity entity = new CustomerEntity(firstWord, secondWord);
                entity.information = remain;
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                CustomerEntity insertedCustomer = result.Result as CustomerEntity;
            }
            reader.Close();
            response.Close();
        }

        private async Task DeleteAllEntity()
        {

            BlobContainerClient container = new BlobContainerClient(connectionString, "mcchowcontainer");

            
            try
            {
                BlobClient blob = container.GetBlobClient("mcchowfile.txt");
                blob.Delete();
            }
            catch(Exception ex)
            {

            }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("css436mcchownametoinfo");
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            var queryResult = table.ExecuteQuery(query);
            foreach (CustomerEntity customerEntity in queryResult)
            {
                TableOperation deleteOperation = TableOperation.Delete(customerEntity);
                TableResult result = await table.ExecuteAsync(deleteOperation);
            }
        }

        public static async Task DeleteEntityAsync(CloudTable table, CustomerEntity deleteEntity)
        {
            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                TableResult result = await table.ExecuteAsync(deleteOperation);

            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                RegisterAsyncTask(new PageAsyncTask(InsertOrMergeEntityAsync));
                Label1.Text = "done Load";
            }

            catch (Exception ex)
            {
                Label1.Text = ex.Message;
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            try
            {

                RegisterAsyncTask(new PageAsyncTask(DeleteAllEntity));
                Label1.Text = "done Clear";
            }

            catch (Exception ex)
            {
                Label1.Text = ex.Message;
            }
        }

        private async Task FindEntity()
        {
                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("css436mcchownametoinfo");
            string partitionKey;
            string rowKey;
            partitionKey = TextBox1.Text;
            rowKey = TextBox2.Text;
            Label1.Text = "";
            if (partitionKey == "" && rowKey != "")
            {
                TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));

                var queryResult = table.ExecuteQuery(query);
                foreach (CustomerEntity customerEntity in queryResult)
                {
                    Label1.Text += customerEntity.PartitionKey + " " + customerEntity.RowKey + " " + customerEntity.information + "<br/>";
                }
            }
            else if (partitionKey != "" && rowKey == "")
            {
                TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

                var queryResult = table.ExecuteQuery(query);
                foreach (CustomerEntity customerEntity in queryResult)
                {
                    Label1.Text += customerEntity.PartitionKey + " "+ customerEntity.RowKey + " " + customerEntity.information + "<br/>";
                }
            }
            else
            {
                try
                {
                    TableOperation retrieveOperation = TableOperation.Retrieve<CustomerEntity>(partitionKey, rowKey);
                    TableResult result = await table.ExecuteAsync(retrieveOperation);
                    CustomerEntity customer = result.Result as CustomerEntity;
                    if (customer != null)
                    {

                        Label1.Text = partitionKey + " "  + rowKey + " " + customer.information;
                    }
                    else
                    {
                        Label1.Text = "Data not found";
                    }
                }
                catch (Exception ex)
                {
                    Label1.Text = "Data not found";
                }
            }


        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            try
            {

                RegisterAsyncTask(new PageAsyncTask(FindEntity));
                //Label1.Text = "done";
            }
            catch (Exception ex)
            {
                Label1.Text = ex.Message;
            }
        }
    }
}