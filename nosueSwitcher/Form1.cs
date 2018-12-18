using MetroFramework;
using MetroFramework.Forms;
using nosueSwitcher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nosueSwitcher
{


    public partial class Form1 : MetroForm
    {
        
    public string nosueDNS = "220.95.232.123";
        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            (new Form2()).Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Check and install certificate
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, "osu! Private Server Public CA", true);

            if (certs.Count > 0)
            {
                metroToggle2.Checked = true;
            }
            try
            {
                if (Dns.GetHostAddresses("nosue.conntest").Length > 0)
                {
                    metroToggle1.Checked = true;
                }
            }
            catch (Exception)
            {
            }
            try
            {
                if (Dns.GetHostAddresses("0.1.updateclient.0su.kr").Length > 0)
                {
                    DialogResult dr = MetroMessageBox.Show(this, "Seems like new client has been released.\nDo you want to download now?", "Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        Process.Start("http://nosue.win/");
                    }
                }
            } catch
            {

            }
            backgroundWorker5.RunWorkerAsync();

        }

        /// <summary>
        /// Set's the DNS Server of the local machine
        /// </summary>
        /// <param name="nic">NIC address</param>
        /// <param name="dnsServers">Comma seperated list of DNS server addresses</param>
        /// <remarks>Requires a reference to the System.Management namespace</remarks>
        public void SetNameservers(string dnsServers)
        {
            using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    foreach (var managementObject in networkConfigs.Cast<ManagementObject>())
                    {
                        using (var newDNS = managementObject.GetMethodParameters("SetDNSServerSearchOrder"))
                        {
                            newDNS["DNSServerSearchOrder"] = dnsServers.Split(',');
                            managementObject.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                        }
                    }
                }
            }
        }

        public String installCertificate()
        {
            try
            {

                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                // Save the certificate in settingsPath temporary
                string certFilePath = System.IO.Path.GetTempPath() + "\\certificate.cer";
                File.WriteAllBytes(certFilePath, Resources.certificate);

                // Get all certficates
                X509Certificate2Collection collection = new X509Certificate2Collection();
                collection.Import(certFilePath);
                    store.Add(collection[0]);
                // Delete temp certificate file
                File.Delete(certFilePath);

                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            SetNameservers("220.95.232.123");
            Process.Start("ipconfig", "/flushdns");
            Invoke(new MethodInvoker(delegate ()
            {

                metroLabel1.Text = "Done!";
                metroProgressSpinner1.Spinning = false;
            }));
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {

            SetNameservers("8.8.8.8");
            Process.Start("ipconfig", "/flushdns");
            Invoke(new MethodInvoker(delegate ()
            {

                metroLabel1.Text = "Done!";
                metroProgressSpinner1.Spinning = false;
            }));
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {

            String certificateResult = installCertificate();
            if(certificateResult == "")
            {
                //Success

                Invoke(new MethodInvoker(delegate ()
                {

                    metroLabel1.Text = "Done!";
                    metroProgressSpinner1.Spinning = false;
                }));
            } else
            {

                Invoke(new MethodInvoker(delegate ()
                {

                    metroLabel1.Text = "Failed: " + certificateResult;
                    metroProgressSpinner1.Spinning = false;
                    metroToggle2.Checked = !metroToggle2.Checked;
                }));
            }
        }

        private void metroToggle2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

                store.Open(OpenFlags.ReadWrite);
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, "osu! Private Server Public CA", true);
                    store.Remove(certs[0]);
                Invoke(new MethodInvoker(delegate ()
                {

                    metroLabel1.Text = "Done!";
                    metroProgressSpinner1.Spinning = false;
                }));
            }
            catch (Exception e2)
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    metroLabel1.Text = "Failed: " + e2.Message;
                    metroProgressSpinner1.Spinning = false;
                    metroToggle2.Checked = !metroToggle2.Checked;
                }));
            }
        }

        private void metroToggle2_Click(object sender, EventArgs e)
        {
            if (metroToggle2.Checked)
            {
                backgroundWorker3.RunWorkerAsync();
            }
            else
            {
                backgroundWorker4.RunWorkerAsync();
            }
        }

        private void metroToggle1_Click(object sender, EventArgs e)
        {

            if (metroToggle1.Checked)
            {
                if (!metroToggle2.Checked)
                {

                    DialogResult dr = MetroMessageBox.Show(this, "You didn't installed HTTPS Certificate.\nIt is required to connect with osu! stable/beta/cutting edge or osu!lazer.\nContinue anyway?", "Certificate Error!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (!(dr == DialogResult.Yes))
                    {
                        metroToggle1.Checked = false;
                        return;
                    }
                    else
                    {

                        metroLabel1.Text = "Switching anyway...";
                        metroProgressSpinner1.Spinning = true;
                        backgroundWorker1.RunWorkerAsync();
                    }
                }
                else
                {

                    metroLabel1.Text = "Switching...";
                    metroProgressSpinner1.Spinning = true;
                    backgroundWorker1.RunWorkerAsync();
                }

            }
            else
            {

                metroLabel1.Text = "Switching...";
                metroProgressSpinner1.Spinning = true;
                backgroundWorker2.RunWorkerAsync();
            }

        }

        private void backgroundWorker5_DoWork(object sender, DoWorkEventArgs e)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                metroLabel1.Text = "Updating IP Addresses..";
                metroProgressSpinner1.Spinning = true;
            }));
            try
            {
                var nosueIPs = Dns.GetHostAddresses("dnsserver.0su.kr");
                if (nosueIPs.Length > 0)
                {

                    nosueDNS = nosueIPs[0].ToString();
                    Invoke(new MethodInvoker(delegate ()
                    {


                        metroTextBox1.Text = nosueDNS;
                        metroLabel1.Text = "Done!";
                        metroProgressSpinner1.Spinning = false;
                    }));
                }
                else
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        metroLabel1.Text = "Failed: No valid IP found! Please check nosue.win for more details";
                        metroProgressSpinner1.Spinning = false;
                    }));
                }
            }
            catch (Exception e2)
            {

                Invoke(new MethodInvoker(delegate ()
                {
                    metroLabel1.Text = "Failed: " + e2.Message;
                    metroProgressSpinner1.Spinning = false;
                }));
            }

        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            backgroundWorker5.RunWorkerAsync();
        }
    }

}