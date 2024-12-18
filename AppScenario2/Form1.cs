using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using RestSharp;
using RestSharp.Serializers;

namespace AppScenario2
{
    public partial class Form1 : Form
    {

        string baseURI = @"http://localhost:5676";


        RestClient client = null;

        public Form1()
        {
            InitializeComponent();
            client = new RestClient(baseURI);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Application> list = new List<Application>();

            var request = new RestRequest("/api/SOMIOD/",Method.Get);
            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("Accept", "application/xml");

            try
            {
                var response = client.Execute(request);
                richTextBox1.Clear();
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // Parse the XML content
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(response.Content);

                    // Select all <Application> nodes
                    XmlNodeList appNodes = xmlDoc.SelectNodes("//Application");

                    if (appNodes != null && appNodes.Count > 0)
                    {
                        foreach (XmlNode node in appNodes)
                        {
                            string id = node.SelectSingleNode("Id")?.InnerText;
                            string name = node.SelectSingleNode("Name")?.InnerText;
                            string creationDate = node.SelectSingleNode("CreationDateTime")?.InnerText;

                            // Append to the RichTextBox
                            richTextBox1.AppendText($"Name: {name}\n");
                            richTextBox1.AppendText($"Id: {id}\n");
                            richTextBox1.AppendText($"Creation DateTime: {creationDate}\n");
                            richTextBox1.AppendText("--------------------------\n");
                        }
                    }
                }
                else
                {
                    richTextBox1.AppendText("No data retrieved.\n");
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error: {ex.Message}\n");
            }



        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Check if the input is null or empty
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Name cannot be null or empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop further execution
            }

            // Create the REST request
            var request = new RestRequest("/api/SOMIOD/{name}", Method.Get);
            request.AddUrlSegment("name", textBox2.Text);
            request.AddHeader("Accept", "application/xml");

            try
            {
                var response = client.Execute(request);
                richTextBox1.Clear();

                // Check if the response is successful and has content
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // Parse the XML content
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(response.Content);

                    // Select the first <Application> node
                    XmlNode appNode = xmlDoc.SelectSingleNode("//Application");

                    if (appNode != null)
                    {
                        string id = appNode.SelectSingleNode("Id")?.InnerText;
                        string name = appNode.SelectSingleNode("Name")?.InnerText;
                        string creationDate = appNode.SelectSingleNode("CreationDateTime")?.InnerText;

                        // Display in the RichTextBox
                        richTextBox1.AppendText($"Name: {name}\n");
                        richTextBox1.AppendText($"Id: {id}\n");
                        richTextBox1.AppendText($"Creation DateTime: {creationDate}\n");
                    }
                    else
                    {
                        richTextBox1.AppendText("No application found.\n");
                    }
                }
                else
                {
                    richTextBox1.AppendText("No data retrieved.\n");
                }
            }
            catch (Exception ex)
            {
                // Display error message
                MessageBox.Show($"Error: {ex.Message}", "Request Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Check if the input is null or empty
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Name cannot be null or empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create the REST request
            var request = new RestRequest("/api/SOMIOD/{name}/", Method.Get);
            request.AddUrlSegment("name", textBox2.Text);
            request.AddHeader("Accept", "application/xml");
            request.AddHeader("somiod-locate", "true"); // Custom header for locating containers

            try
            {
                var response = client.Execute(request);
                richTextBox1.Clear();

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // Parse the XML response
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(response.Content);

                    // Search for 'Container' nodes inside the response
                    XmlNodeList containerNodes = xmlDoc.SelectNodes("//Container");

                    if (containerNodes != null && containerNodes.Count > 0)
                    {
                        foreach (XmlNode container in containerNodes)
                        {
                            string id = container.SelectSingleNode("Id")?.InnerText;
                            string name = container.SelectSingleNode("Name")?.InnerText;
                            string creationDate = container.SelectSingleNode("CreationDateTime")?.InnerText;

                            // Append container details to the RichTextBox
                            richTextBox1.AppendText($"Container Name: {name}\n");
                            richTextBox1.AppendText($"Container Id: {id}\n");
                            richTextBox1.AppendText($"Creation DateTime: {creationDate}\n");
                            richTextBox1.AppendText("--------------------------\n");
                        }
                    }
                    else
                    {
                        richTextBox1.AppendText("No containers found inside the application.\n");
                    }
                }
                else
                {
                    richTextBox1.AppendText("No data retrieved or an error occurred.\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Request Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(deletePath.Text))
            {
                MessageBox.Show("Path cannot be null or empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var request = new RestRequest(deletePath.Text, Method.Delete);

            try
            {
                var response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    MessageBox.Show("Resource deleted successfully.\n");
                }
                else
                {
                    MessageBox.Show("Resource not found or an error occurred.\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Request Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
