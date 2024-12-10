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
using RestSharp;
using RestSharp.Serializers;

namespace AppScenario2
{
    public partial class Form1 : Form
    {

        string baseURI = @"http://localhost:5676/";


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

            var request = new RestRequest("api/SOMIOD/",Method.Get);
            request.RequestFormat = DataFormat.Xml;

            try
            {
                var response = client.Execute(request);
                richTextBox1.Clear();
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    richTextBox1.Text = response.Content;
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
            var request = new RestRequest("api/SOMIOD/{name}", Method.Get);
            request.AddUrlSegment("name", textBox2.Text);
            request.AddHeader("Accept", "application/xml");


            try
            {
                var response = client.Execute(request);
                richTextBox1.Clear();
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    richTextBox1.Text = response.Content;
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
    }
}
