using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Data.SqlClient;
using SQLXMLBULKLOADLib;

namespace WindowsApplication1
{
    public partial class frm_IceCat_Main : Form
    {
        public frm_IceCat_Main()
        {
            InitializeComponent();
        }

        static class GlobalClass
        {
            private static string _globalVar;// = "0";

            public static string GlobalVar
            {
                get { return _globalVar; }
                set { _globalVar = value; }
            }

            private static string _globalLanguage;// = "0";

            public static string GlobalLanguage
            {
                get { return _globalLanguage; }
                set { _globalLanguage = value; }
            }

        }

        private void ImageDownLoadBatch()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;

            richTextBox1.AppendText(DateTime.Now + " Starting Image Batch Download process \r");
            Application.DoEvents();

            SqlConnection myConnection2 = new SqlConnection("user id=BasWeb_Appuser;" +
                                       "password=?+0m!Z2x;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=30");

            SqlDataReader SqlR2 = null;
            SqlCommand SqlCmd2 = new SqlCommand("Select count(P.Prod_id) as RecCnt1 from dt_product P "
                            + " inner join IceCat_ProductsMap IPM on P.Prod_id = IPM.Prod_id "
                            + " left join SRVNLD0001.BASPRODUCTION.dbo.F4101 IM on IM.IMAITM = P.Prod_id"
                            + " where languageid=2 and processed=1 and LTRIM(rtrim(HighPic)) <> '' and cast(IMITM as varchar) not in (select distinct BAS_ProductID from dt_ProductImages where ImageQuality = 'High')", myConnection2);

            SqlCmd2.CommandTimeout = 240;

            string tmpresult;
            int RecordCountImages;

            RecordCountImages = 1;

            while (RecordCountImages > 0)
            { 
                try
                {
                    myConnection2.Close();
                    SqlR2.Close();
                }
                catch
                {
                    // Just means connection was left open
                }

                try
                {
                    myConnection2.Open();
                    SqlR2 = SqlCmd2.ExecuteReader();
                }
                catch (Exception ex)
                {
                    richTextBox1.AppendText("Exception occured: " + ex.Message + " \r");
                    ErrCnt = ErrCnt + 1;
                }
              
                SqlR2.Read();
                tmpresult = SqlR2["RecCnt1"].ToString() ;
                
                try
                {
                    RecordCountImages = Convert.ToInt32(tmpresult);
                }
                catch 
                {
                    RecordCountImages = 0;
                }

                richTextBox1.AppendText(DateTime.Now + " Images unprocessed: " + tmpresult + " \r");
    
                if (RecordCountImages > 0)
                {
                    DownloadProductImages();
                }

                try
                {
                }
                catch (Exception ex)
                {
                    richTextBox2.AppendText("Images batch process error: " + ex.Message + " \r");
                }

            }

            Application.DoEvents();

            try
            {
                SqlR2.Close();
                myConnection2.Close();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Exception occured: " + ex.Message + "\r");
            }
            richTextBox1.AppendText(DateTime.Now + " Download images batch completed \r");
        }

        private void DownloadProductImages()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;
             
            richTextBox1.AppendText(DateTime.Now + " Started fetching all available large pictures from ICECAT \r");
            Application.DoEvents();

            SqlConnection myConnection = new SqlConnection("user id=BasWeb_Appuser;" +
                                       "password=?+0m!Z2x;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=30");

            SqlDataReader SqlR1 = null;
            SqlCommand SqlCmd1 = new SqlCommand("Select distinct top 150 P.Prod_id as ManufacturerID, isnull(IMITM,1) as IMITM, HighPic from dt_product P "
                            + " inner join IceCat_ProductsMap IPM on P.Prod_id = IPM.Prod_id "
                            + " left join SRVNLD0001.BASPRODUCTION.dbo.F4101 IM on IM.IMAITM = P.Prod_id"
                            + " where languageid=2 and processed=1 and LTRIM(rtrim(HighPic)) <> '' and cast(IMITM as varchar) not in (select distinct BAS_ProductID from dt_ProductImages where ImageQuality = 'High')", myConnection);

            SqlCmd1.CommandTimeout = 240;

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }

            try
            {
                myConnection.Open();
                SqlR1 = SqlCmd1.ExecuteReader();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("Exception occured: " + ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }

            string FleURL;
            string ManufactID;
            string BasProductID;
            string Quality;
            int Sequence;

            richTextBox1.AppendText(DateTime.Now + " Started inserting all large pictures into SQL \r");
            Application.DoEvents();

            while (SqlR1.Read())
            {
                FleURL = (SqlR1["HighPic"].ToString());
                ManufactID = (SqlR1["ManufacturerID"].ToString());
                BasProductID = (SqlR1["IMITM"].ToString());
                Quality = "High";
                Sequence = 1;
                try
                {
                    DownloadICECATImage(FleURL, txtB_User.Text, txtB_Pass.Text, ManufactID, BasProductID, Quality, Sequence);
                }
                catch (Exception ex)
                {
                    richTextBox2.AppendText("SQL Loop large pics exception occured: " + ex.Message + " \r");
                    richTextBox2.AppendText("Proces info URL: " + FleURL + "\r");
                    richTextBox2.AppendText("Proces info File: " + ManufactID + "\r");
                }
                
            }

            Application.DoEvents();

            try
            {
                SqlR1.Close();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("Exception occured: " + ex.Message + "\r");
            }
            richTextBox1.AppendText(DateTime.Now + " Download large pictures done \r");
        }


        public static Byte[] DownloadICECATImage(string strDownloadURL, string strUser, string strPWD, string ManufactID, string BasProductID, string Quality, int Sequence )
        {

            //Daily index string  strDownloadURL = "http://data.icecat.biz/export/level4/EN";
            //string strUser = "bascomputers";
            //string strPWD = "1019asr";
            //string strPath = "c:\\ProductList.xml";

            // Creating an instance of a WebClient
            int ErrCnt;
            ErrCnt = 0;
            WebClient req = new WebClient();

            // Creating an instance of a credential cache,
            // and passing the username and password to it

            CredentialCache cache = new CredentialCache();
            cache.Add(new Uri(strDownloadURL), "Basic", new NetworkCredential(strUser, strPWD));
            req.Credentials = cache;

            try
            {
                Byte[] fileData = req.DownloadData(strDownloadURL) as byte[];
            //    File.WriteAllBytes(strPath, fileData);

                SqlConnection MyConnection = new SqlConnection("user id=sa;password=%kg77hB;server=172.16.0.63;Trusted_Connection=yes;database=ICECAT;connection timeout=30");
                MyConnection.Open();

                SqlCommand MyCommand = new SqlCommand("INSERT INTO dt_ProductImages ([Manufacturer_ID], [BAS_ProductID], [IceCat_Product_ID], [ImageBlob], [ImageQuality], [Sequence]) VALUES (@ManufactID,@BasProductID,0,@ImageData, @Quality, @Sequence)", MyConnection);
                SqlParameter param1 = new SqlParameter("@ManufactID", SqlDbType.VarChar);
                SqlParameter param2 = new SqlParameter("@BasProductID", SqlDbType.VarChar);
                SqlParameter param3 = new SqlParameter("@ImageData", SqlDbType.Image);
                SqlParameter param4 = new SqlParameter("@Quality", SqlDbType.VarChar);
                SqlParameter param5 = new SqlParameter("@Sequence", SqlDbType.VarChar);

                param1.Value = ManufactID;
                param2.Value = BasProductID;
                param3.Value = fileData;
                param4.Value = Quality;
                param5.Value = Sequence;

                MyCommand.Parameters.Add(param1);
                MyCommand.Parameters.Add(param2);
                MyCommand.Parameters.Add(param3);
                MyCommand.Parameters.Add(param4);
                MyCommand.Parameters.Add(param5);

                MyCommand.ExecuteNonQuery();

                return fileData;
            }
            catch (Exception)
            {
                ErrCnt = ErrCnt + 1;
               
                //MessageBox.Show(Ex.Message);
            }
            finally { };

            if (ErrCnt == 0)
            {
                return null;
            }
            else
            {
                return null;
            }
        }


        public static Byte[] DownloadICEcatFile(string strDownloadURL, string strUser, string strPWD, string strPath)
        {

            //Daily index string  strDownloadURL = "http://data.icecat.biz/export/level4/EN";
            //string strUser = "bascomputers";
            //string strPWD = "1019asr";
            //string strPath = "c:\\ProductList.xml";

            // Creating an instance of a WebClient
            int ErrCnt;
            ErrCnt = 0;
            WebClient req = new WebClient();

            // Creating an instance of a credential cache,
            // and passing the username and password to it

            CredentialCache cache = new CredentialCache();
            cache.Add(new Uri(strDownloadURL), "Basic", new NetworkCredential(strUser, strPWD));
            req.Credentials = cache;

            try
            {
                Byte[] fileData = req.DownloadData(strDownloadURL) as byte[];
                File.WriteAllBytes(strPath, fileData);
                return fileData;
            }
            catch (Exception)
            {
                ErrCnt = ErrCnt + 1;
            }
            finally { };

            if (ErrCnt == 0)
            {
                return null ;
            }
            else
            {
                return null ;
            }
        }

        public int DownloadICEcatFileFullNL()
        {
            string strDownloadURL = "http://data.icecat.biz/export/level4/NL/files.index.xml";
            string strUser = "bascomputers";
            string strPWD = "1019asr";
            string strPath = "C:\\userfolder\\Importxml\\ProductListNL.xml";
            int ErrCnt; 
            ErrCnt = 0;
            WebClient req = new WebClient();
            CredentialCache cache = new CredentialCache();
            cache.Add(new Uri(strDownloadURL), "Basic", new NetworkCredential(strUser, strPWD));
            req.Credentials = cache;
            
            try
            {
                Byte[] fileData = req.DownloadData(strDownloadURL) as byte[];
                File.WriteAllBytes(strPath, fileData);
            }
            catch (Exception)
            {
               ErrCnt = ErrCnt + 1;
            }
            finally { };

            if (ErrCnt == 0)
            {
                richTextBox1.AppendText("Finished downloading Full NL \r");
                return 0;
            }
            else
            {
                richTextBox2.AppendText("Errors occured while downloading NL \r");
                return 1;
            }
        }

        public int DownloadICEcatFileFullEN()
        {
            string strDownloadURL = "http://data.icecat.biz/export/level4/EN/files.index.xml";
            string strUser = "bascomputers";
            string strPWD = "1019asr";
            string strPath = "C:\\userfolder\\Importxml\\ProductListEN.xml";
            int ErrCnt;
            ErrCnt = 0;
            WebClient req = new WebClient();
            CredentialCache cache = new CredentialCache();
            cache.Add(new Uri(strDownloadURL), "Basic", new NetworkCredential(strUser, strPWD));
            req.Credentials = cache;
            try
            {
                Byte[] fileData = req.DownloadData(strDownloadURL) as byte[];
                File.WriteAllBytes(strPath, fileData);
            }
            catch (Exception)
            {
                ErrCnt = ErrCnt + 1;
            }
            finally { };

            if (ErrCnt == 0)
            {
                richTextBox1.AppendText(DateTime.Now + "Finished downloading Full EN \r");
                return 0;
            }
            else
            {
                richTextBox2.AppendText(DateTime.Now + "Errors occured while downloading EN \r");
                return 1;
            }

        }

        private void CleanFullImportTable()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;

            SqlConnection myConnection = new SqlConnection("user id=BasWeb_Appuser;" +
                                       "password=?+0m!Z2x;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=120");
            //Process_IceFullTable_AfterImport
            SqlCommand SqlCmd1 = new SqlCommand("truncate table IceCat_ProductsMap_History "
                            + "insert into IceCat_ProductsMap_History select distinct * from IceCat_ProductsMap "
                            + "truncate table IceCat_ProductsMap ", myConnection);
            SqlCmd1.CommandTimeout = 180;

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }
            finally { };

            richTextBox1.AppendText("Starting table cleanup \r");
            Application.DoEvents();

            try
            {
                myConnection.Open();
                SqlCmd1.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Exception occured: " + ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }
            finally { };

            try
            {
                myConnection.Close();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Exception occured: " + ex.Message + "\r");
            }
            finally { };

            if (ErrCnt == 0)
            {
                richTextBox1.AppendText(DateTime.Now + " Cleanup table succesfull \r");
            }
            else
            {
                richTextBox1.AppendText(DateTime.Now + " Error in cleanup table! \r");
                richTextBox2.AppendText(DateTime.Now + " Error in cleanup table! \r");
            }
        }

        private void Process_IceFullTable_AfterImport()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;
            
            SqlConnection myConnection = new SqlConnection("user id=BasWeb_Appuser;" +
                                       "password=?+0m!Z2x;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=120");
            
            SqlCommand SqlCmd1 = new SqlCommand("exec Process_IceFullTable_AfterImport ", myConnection);

            SqlCmd1.CommandTimeout = 600;

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }
            finally { };

            richTextBox1.AppendText("Updating import table \r");
            richTextBox1.ScrollToCaret();
            Application.DoEvents();
            

            try
            {
                myConnection.Open();
                SqlCmd1.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("PIA01 Execute exception occured: " + ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }
            finally { };

            try
            {
                myConnection.Close();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("PIA02 Connection close exception occured: " + ex.Message + "\r");
            }
            finally { };

            if (ErrCnt == 0)
            {
                richTextBox1.AppendText("Update import table done \r");
            }
            else
            {
                richTextBox1.AppendText("Update import table failed with errors! \r");
                richTextBox2.AppendText("Update import table failed with errors! \r");
            }
        }

        private void Process_Replicate_dt_Tables()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;

            SqlConnection myConnection = new SqlConnection("user id=BasWeb_Appuser;" +
                                       "password=?+0m!Z2x;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=120");
            
            SqlCommand SqlCmd1 = new SqlCommand("EXEC MSDB..SP_Start_Job @Job_Name = 'ICECAT.Replicate_dt_Tables' ", myConnection);

            SqlCmd1.CommandTimeout = 600;

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }
            finally { };

            richTextBox1.AppendText("Replication products to dt tables \r");
            richTextBox1.ScrollToCaret();
            Application.DoEvents();

            try
            {
                myConnection.Open();
                SqlCmd1.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("PIA03 Execute exception occured: " + ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }
            finally { };

            try
            {
                myConnection.Close();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("PIA04 Connection close exception occured: " + ex.Message + "\r");
            }
            finally { };

            if (ErrCnt == 0)
            {
                richTextBox1.AppendText(DateTime.Now + " Calling SQL Replicate dt_tables succesfull \r");
                richTextBox1.AppendText("Check SQL log for dt_replicate status \r");
            }
            else
            {
                richTextBox2.AppendText(DateTime.Now + " Error calling SQL Replicate dt_tables! \r");
            }

            richTextBox1.AppendText("Replicating to dt tables done \r");
            richTextBox1.AppendText("Last update: " + DateTime.Now + "\r");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled != false) { timer1.Enabled = false; }
            if (timer_FullImport.Enabled != false) { timer_FullImport.Enabled = false; }
            //ExecuteFullIndexDownload();
//            ProcessProductDetails();
            timer_FullImport.Interval = 10000;
            timer_FullImport.Enabled = true;
            richTextBox1.AppendText("Initializing... \r");
        }

        private void ExecuteFullIndexDownload()
        {
   //         richTextBox1.AppendText("Starting download of full productsmap \r");
    //        richTextBox1.ScrollToCaret();
      //      Application.DoEvents();
        //    CleanFullImportTable();
            //richTextBox2.AppendText("SKIPPING CLEANUP! \r");

            try  //download dutch fullmap
            {
//               DownloadICEcatFile(txtB_DutchFull.Text, txtB_User.Text, txtB_Pass.Text, txtB_Path.Text);
//               ProcessXMLFullFile();
            }
            catch
            {
  //             richTextBox2.AppendText("Error downloading dutch fullmap \r");
            }
            finally { };

            try //download english fullmap
            {
    //           DownloadICEcatFile(txtB_EnglishFull.Text, txtB_User.Text, txtB_Pass.Text, txtB_Path.Text);
      //         ProcessXMLFullFile();
            }
            catch
            {
      //         richTextBox2.AppendText("Error downloading english fullmap \r");
            }
            finally { };

          //  Process_IceFullTable_AfterImport();
            //// richTextBox2.AppendText("\r ERROR ::: Skipped processfull after import!! ::: \r");

        //    richTextBox1.AppendText(DateTime.Now + " Full download finished \r");
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ProcessProductDetails();
        }

        private void ProcessProductDetails()
        {
            timer1.Enabled = false;
            if (timer1.Interval != 100) { timer1.Interval = 100; }
            
            GlobalClass.GlobalVar = "0";
            GlobalClass.GlobalLanguage = "0";

            richTextBox1.AppendText(DateTime.Now + " Product timer (re)started \r");
            //richTextBox2.Clear();
            timer1.Enabled = true;
        }

        private void DownloadAndProcessIceCatProducts(string LangID)
        {
            if (timer1.Enabled != false) { timer1.Enabled = false; }

            int ErrCnt;
            ErrCnt = 0;

            SqlConnection myConnection = new SqlConnection("user id=sa;" +
                                       "password=%kg77hB;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=30");
            try
            {
                myConnection.Open();
            }
            catch (Exception Ex)
            {
                richTextBox2.AppendText("Error connecting: " + Ex.Message + "\r");
                ErrCnt++;
            }
            finally { };

            if (LangID == "1" && GlobalClass.GlobalVar != "1")
            {
                SqlCommand SqlCmdLangID1 = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID') ALTER TABLE [dbo].[Product] DROP CONSTRAINT [DF_SetLangID] ALTER TABLE [dbo].[Product] ADD CONSTRAINT [DF_SetLangID] DEFAULT ((1)) FOR [Langid]", myConnection);
                try { SqlCmdLangID1.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting product languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                SqlCommand SqlCmdLangID1a = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID_SummaryDescription') ALTER TABLE [dbo].[SummaryDescription] DROP CONSTRAINT [DF_SetLangID_SummaryDescription] ALTER TABLE [dbo].[SummaryDescription] ADD CONSTRAINT [DF_SetLangID_SummaryDescription] DEFAULT ((1)) FOR [Langid]", myConnection);
                try { SqlCmdLangID1a.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting summarydescription languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                SqlCommand SqlCmdLangID1b = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID_PF') ALTER TABLE [dbo].[Productfeature] DROP CONSTRAINT [DF_SetLangID_PF] ALTER TABLE [dbo].[Productfeature] ADD CONSTRAINT [DF_SetLangID_PF] DEFAULT ((1)) FOR [Langid]", myConnection);
                try { SqlCmdLangID1b.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting ProductFeature languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                richTextBox1.AppendText(DateTime.Now + " Switched to English content \r");
                GlobalClass.GlobalVar = "1";
            }
            else if (LangID == "2" && GlobalClass.GlobalVar != "2")
            {
                SqlCommand SqlCmdLangID2 = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID') ALTER TABLE [dbo].[Product] DROP CONSTRAINT [DF_SetLangID] ALTER TABLE [dbo].[Product] ADD CONSTRAINT [DF_SetLangID] DEFAULT ((2)) FOR [Langid]", myConnection);
                try { SqlCmdLangID2.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                SqlCommand SqlCmdLangID2a = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID_SummaryDescription') ALTER TABLE [dbo].[SummaryDescription] DROP CONSTRAINT [DF_SetLangID_SummaryDescription] ALTER TABLE [dbo].[SummaryDescription] ADD CONSTRAINT [DF_SetLangID_SummaryDescription] DEFAULT ((2)) FOR [Langid]", myConnection);
                try { SqlCmdLangID2a.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting summarydescription languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                SqlCommand SqlCmdLangID1b = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID_PF') ALTER TABLE [dbo].[Productfeature] DROP CONSTRAINT [DF_SetLangID_PF] ALTER TABLE [dbo].[Productfeature] ADD CONSTRAINT [DF_SetLangID_PF] DEFAULT ((2)) FOR [Langid]", myConnection);
                try { SqlCmdLangID1b.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting ProductFeature languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                richTextBox1.AppendText(DateTime.Now + " Switched to Dutch content \r");
                GlobalClass.GlobalVar = "2";
            }
            else if (LangID == "3" && GlobalClass.GlobalVar != "3")
            {
                SqlCommand SqlCmdLangID3 = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID') ALTER TABLE [dbo].[Product] DROP CONSTRAINT [DF_SetLangID] ALTER TABLE [dbo].[Product] ADD CONSTRAINT [DF_SetLangID] DEFAULT ((3)) FOR [Langid]", myConnection);
                try { SqlCmdLangID3.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting [product languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };
  
                SqlCommand SqlCmdLangID3a = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID_SummaryDescription') ALTER TABLE [dbo].[SummaryDescription] DROP CONSTRAINT [DF_SetLangID_SummaryDescription] ALTER TABLE [dbo].[SummaryDescription] ADD CONSTRAINT [DF_SetLangID_SummaryDescription] DEFAULT ((3)) FOR [Langid]", myConnection);
                try { SqlCmdLangID3a.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting summarydescription languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                SqlCommand SqlCmdLangID1b = new SqlCommand("IF Exists (select Name from sys.objects where name = 'DF_SetLangID_PF') ALTER TABLE [dbo].[Productfeature] DROP CONSTRAINT [DF_SetLangID_PF] ALTER TABLE [dbo].[Productfeature] ADD CONSTRAINT [DF_SetLangID_PF] DEFAULT ((3)) FOR [Langid]", myConnection);
                try { SqlCmdLangID1b.ExecuteNonQuery(); }
                catch (Exception Ex) { richTextBox2.AppendText("Error setting ProductFeature languagedefault: " + Ex.Message + "\r"); ErrCnt++; }
                finally { };

                richTextBox1.AppendText(DateTime.Now + " Switched to French content \r");
                GlobalClass.GlobalVar = "3";
            }
            else
            {
                // Language default for product table already set or 0
            }


            SqlDataReader SqlR1 = null;
            SqlCommand SqlCmd1 = new SqlCommand("Select path from IceCat_ProductsMap where Processed = 4 and LanguageID=" + LangID , myConnection); 
            //Where (Processed is null) and Prod_ID in (Select IMAITM from SRVNLD0001.BASPRODUCTION.dbo.F4101 where IMSTKT IN ('U','S'))", myConnection);
            String RecUpd;
            // top 1 kills performance???

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }
            finally { };
            
            try
            {
                    myConnection.Open();
                    SqlR1 = SqlCmd1.ExecuteReader();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("DB open Exception occured: " + ex.Message + " \r");
                ErrCnt++;
            }
            finally { };

            try
            {
                if (SqlR1.Read())
                {
                    //richTextBox1.AppendText("Downloading: http://data.icecat.biz/" + (SqlR1["path"].ToString()) + "\r");
                    if (timer1.Interval > 100)
                    {
                        timer1.Interval = 100;
                    }

                    try
                    {
                        DownloadICEcatFile(("http://data.icecat.biz/" + SqlR1["path"].ToString()), txtB_User.Text, txtB_Pass.Text, txtB_Detail.Text);
                    }
                    catch (Exception ex)
                    {
                        ErrCnt++;
                        richTextBox2.AppendText("Exception occured: " + ex.Message + " \r");
                        richTextBox2.AppendText("Exception on: " + SqlR1["path"].ToString() + "\r");
                    }

                    //richTextBox1.AppendText("Processing: http://data.icecat.biz/" + (SqlR1["path"].ToString()) + "\r");
                 
                    try
                    {
                        ProcessXMLProductFile();
                    }
                    catch (Exception Ex)
                    {
                        ErrCnt++;
                        richTextBox2.AppendText("Exception occured: " + Ex.Message + " \r");
                        richTextBox2.AppendText("Exception on: " + SqlR1["path"].ToString() + "\r");
                    }

                    richTextBox1.AppendText("Finished: " + (SqlR1["path"].ToString()) + "\r");

                    SqlCommand SqlCmd2 = new SqlCommand("Update IceCat_ProductsMap set Processed=1 WHERE path='" + (SqlR1["path"].ToString()) + "'", myConnection);
                    SqlR1.Close();
                    if (ErrCnt == 0)
                    {
                        RecUpd = Convert.ToString(SqlCmd2.ExecuteNonQuery());
                    }
                    //richTextBox1.AppendText("Rows updated: " + RecUpd);
                }
                else
                {
                    if (GlobalClass.GlobalVar == "1")
                    { 
                        //timer1.Enabled = false;
                        GlobalClass.GlobalLanguage = "2";
                        //ErrCnt++;
                        ErrCnt = 0;
                        richTextBox1.AppendText("Starting next language (Dutch) \r");
                    }
                    else if (GlobalClass.GlobalVar == "2")
                    {
                        ErrCnt++;
                        //GlobalClass.GlobalLanguage = "3";
                        //richTextBox2.AppendText("Starting next language (French) \r");

                        richTextBox1.AppendText("No more un-processed records found. \r");
                        //timer1.Interval = 600000;
                        //timer1.Enabled = true;
                        timer1.Enabled = false;
                        GlobalClass.GlobalLanguage = "0";
                        richTextBox1.AppendText("Product timer disabled. \r");
                        //richTextBox1.AppendText("Product timer disabled. \r");
                        //Call SQL agent job
                        Process_Replicate_dt_Tables();
                        //richTextBox2.AppendText("Replicate tables skipped!! \r");
                    }
                    else if (GlobalClass.GlobalVar == "3")
                    {
                        //timer3.Enabled = false; 
                    }

                }
            }
            catch (Exception Ex)
            {
                richTextBox2.AppendText("General exception occured: " + Ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }
            finally { };

            try 
            {
                myConnection.Close();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("Exception occured: " + ex.Message + "\r");
            }
            finally { };

            if (ErrCnt == 0) // && LangID == "1")
            {
                timer1.Enabled = true;
            }
            //else if (ErrCnt == 0 && LangID == "2")
            //{
                //timer1.Enabled = true;
            //}

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (GlobalClass.GlobalLanguage != "0")
            {
                DownloadAndProcessIceCatProducts(GlobalClass.GlobalLanguage);
            }
            else
            {
                GlobalClass.GlobalLanguage = "1";
                GlobalClass.GlobalVar = "0";
                richTextBox1.AppendText("Starting first run \r");
            }
        }


        private void timer_FullImport_Tick(object sender, EventArgs e)
        {
            if (timer1.Enabled != false) { timer1.Enabled = false; }

            if (timer_FullImport.Interval != 10800000)
            {
                timer_FullImport.Interval = 10800000;
                richTextBox1.AppendText("\r \r");
                richTextBox1.AppendText(DateTime.Now + "\r"); 
                richTextBox1.AppendText("Full import timer set to every 3 hours \r");
                richTextBox1.AppendText("Running now! \r");
            }
            else
            {
                richTextBox1.AppendText("\r \r");
                richTextBox1.AppendText(DateTime.Now + "\r"); 
                richTextBox1.AppendText("Starting normal full import run \r");
            }

            CleanFullImportTable();

            DownloadICEcatFileFullNL();
            ProcessXMLFullFileNL();
            DownloadICEcatFileFullEN();
            ProcessXMLFullFileEN();

            Process_IceFullTable_AfterImport();

            ProcessProductDetails();

//            ExecuteFullIndexDownload();
//            ProcessProductDetails();
        }


        private void DownloadPictures()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;

            SqlConnection myConnection = new SqlConnection("user id=sa;" +
                                       "password=%kg77hB;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=30");

            SqlDataReader SqlR1 = null;
            SqlCommand SqlCmd1 = new SqlCommand("Select distinct sku as ProductID, ThumbPic as thumb_pic from Haalnuweg_Jaap", myConnection);
            // top 1 kills performance???

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }
            finally { }

            try
            {
                myConnection.Open();
                SqlR1 = SqlCmd1.ExecuteReader();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("Exception occured: " + ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }
            finally { }

            string FleURL;
            string FleName;
            
            richTextBox1.AppendText("Started downloading all thumbs \r");
            Application.DoEvents();

            while (SqlR1.Read())
            {
                FleURL = (SqlR1["thumb_pic"].ToString());
                FleName = ("C:\\userfolder\\ICECAT\\Generated\\Thumbs\\" + SqlR1["ProductID"].ToString() + ".jpg" );
                try
                {
                    DownloadICEcatFile(FleURL, txtB_User.Text, txtB_Pass.Text, FleName);
                }
                catch (Exception ex)
                {
                    richTextBox2.AppendText("SQL Loop thumbnails exception occured: " + ex.Message + " \r");
                    richTextBox2.AppendText("Proces info URL: " + FleURL + "\r");
                    richTextBox2.AppendText("Proces info File: " + FleName + "\r");
                }
                finally { };
                Application.DoEvents();
             }

             try
             {
                 SqlR1.Close();
                 myConnection.Close();
             }
             catch (Exception ex)
             {
                 richTextBox2.AppendText("Exception occured: " + ex.Message + "\r");
             }
             finally { };
             richTextBox1.AppendText("Download thumbs done \r");
        }

        private void ProcessXMLProductFile()
        {
            try
            {
  //              richTextBox2.AppendText("Start File2SQL Bulk Import \r");
                string XSDFilename, XMLFilename;

                XSDFilename = "C:\\userfolder\\icecat\\IceCat_ProductDetails\\productdetailsmap.xml";
                XMLFilename = txtB_Detail.Text;

                SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class objBL = new SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class();
                objBL.ConnectionString = "Provider=SQLOLEDB;server=172.16.0.63;database=ICECAT;uid=sa;pwd=%kg77hB;integrated security=SSPI";
                objBL.ErrorLogFile = "C:\\userfolder\\Importxml\\error.log";
                objBL.KeepIdentity = false;
                //            objBL.CheckConstraints = false;
                //            objBL.IgnoreDuplicateKeys = false;
                //            objBL.KeepNulls = true;
                objBL.Execute(XSDFilename, XMLFilename);
  //              richTextBox2.AppendText("Finished File2SQL Bulk Import \r");
            }
            catch(Exception Ex)
            {
                richTextBox2.AppendText("ProcessXMLFile SQLBulk error occured:  \r" + Ex.Message);
                try
                {
                    File.Copy(txtB_Detail.Text, txtB_Detail.Text + Guid.NewGuid().ToString() + ".err");
                    richTextBox2.AppendText(txtB_Detail.Text + ".err file created \r");
                }
                catch (Exception)
                {
                    richTextBox2.AppendText(txtB_Detail.Text +  ".err Unable to create new error file! \r");
                }
            }


        }

        private void ProcessXMLFullFile()
        {
            richTextBox1.AppendText("Starting to import FULL ICECAT product xml \r");
            richTextBox1.AppendText("This may take several minutes... \r");
            try
            {
                //              richTextBox2.AppendText("Start File2SQL Bulk Import \r");
                string XSDFilename, XMLFilename;

                XSDFilename = "C:\\userfolder\\ICECAT\\Production\\Main IceCat Index 2 SQL\\ProductListMap.xml";
                XMLFilename = txtB_Path.Text ; 
           
                SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class objBL = new SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class();
                objBL.ConnectionString = "Provider=SQLOLEDB;server=172.16.0.63;database=ICECAT;uid=sa;pwd=%kg77hB;integrated security=SSPI";
                objBL.ErrorLogFile = "C:\\userfolder\\Importxml\\error.log";
                objBL.KeepIdentity = false;
                //            objBL.CheckConstraints = false;
                //            objBL.IgnoreDuplicateKeys = false;
                //            objBL.KeepNulls = true;
                objBL.Execute(XSDFilename, XMLFilename);
                //              richTextBox2.AppendText("Finished File2SQL Bulk Import \r");
                richTextBox1.AppendText("Full import finished executing succesfull. \r");

                File.Delete(txtB_Path.Text);
            }
            catch (Exception Ex)
            {
                richTextBox2.AppendText("ProcessXMLFile SQLBulk2 error occured: \r" + Ex.Message);
            }


        }
        
        private void ProcessXMLFullFileNL()
        {
            richTextBox1.AppendText("Starting to import NL FULL ICECAT product xml \r");
            richTextBox1.AppendText("This may take several minutes... \r");
            try
            {
                richTextBox1.AppendText("Start File2SQL Bulk Import NL\r");
                string XSDFilename, XMLFilename;

                XSDFilename = "C:\\userfolder\\ICECAT\\Production\\Main IceCat Index 2 SQL\\ProductListMap.xml";
                XMLFilename = "C:\\userfolder\\Importxml\\ProductListNL.xml"; 
           
                SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class objBL = new SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class();
                objBL.ConnectionString = "Provider=SQLOLEDB;server=172.16.0.63;database=ICECAT;uid=sa;pwd=%kg77hB;integrated security=SSPI";
                objBL.ErrorLogFile = "C:\\userfolder\\Importxml\\errorNL.log";
                objBL.KeepIdentity = false;
                //            objBL.CheckConstraints = false;
                //            objBL.IgnoreDuplicateKeys = false;
                //            objBL.KeepNulls = true;
                objBL.Execute(XSDFilename, XMLFilename);
                //              richTextBox2.AppendText("Finished File2SQL Bulk Import \r");
                richTextBox1.AppendText("Full NL import finished executing succesfull. \r");

                File.Delete(txtB_Path.Text);
            }
            catch (Exception Ex)
            {
                richTextBox1.AppendText("ProcessXMLFile SQLBulk2 error occured: " + Ex.Message + "\r");
                richTextBox2.AppendText("ProcessXMLFile SQLBulk2 error occured: " + Ex.Message + "\r");
            }


        }

        private void ProcessXMLFullFileEN()
        {
            richTextBox1.AppendText("Starting to import EN FULL ICECAT product xml \r");
            richTextBox1.AppendText("This may take several minutes... \r");
            try
            {
                richTextBox1.AppendText("Start File2SQL Bulk Import EN\r");
                string XSDFilename, XMLFilename;

                XSDFilename = "C:\\userfolder\\ICECAT\\Production\\Main IceCat Index 2 SQL\\ProductListMap.xml";
                XMLFilename = "C:\\userfolder\\Importxml\\ProductListEN.xml";

                SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class objBL = new SQLXMLBULKLOADLib.SQLXMLBulkLoad3Class();
                objBL.ConnectionString = "Provider=SQLOLEDB;server=172.16.0.63;database=ICECAT;uid=sa;pwd=%kg77hB;integrated security=SSPI";
                objBL.ErrorLogFile = "C:\\userfolder\\Importxml\\errorEN.log";
                objBL.KeepIdentity = false;
                //            objBL.CheckConstraints = false;
                //            objBL.IgnoreDuplicateKeys = false;
                //            objBL.KeepNulls = true;
                objBL.Execute(XSDFilename, XMLFilename);
                //              richTextBox2.AppendText("Finished File2SQL Bulk Import \r");
                richTextBox1.AppendText("Full EN import finished executing succesfull. \r");

                File.Delete(txtB_Path.Text);
            }
            catch (Exception Ex)
            {
                richTextBox1.AppendText("ProcessXMLFile EN SQLBulk2 error occured: \r" + Ex.Message);
                richTextBox2.AppendText("ProcessXMLFile EN SQLBulk2 error occured: \r" + Ex.Message);
            }


        }
        

        private void button5_Click(object sender, EventArgs e)
        {
            ImageDownLoadBatch();
        }
    

    private void DownloadHighPictures()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;

            SqlConnection myConnection = new SqlConnection("user id=sa;" +
                                       "password=%kg77hB;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=30");

            SqlDataReader SqlR1 = null;
            SqlCommand SqlCmd1 = new SqlCommand("Select distinct sku as ProductID, HighPic as high_pic from Haalnuweg_Jaap", myConnection);
            // top 1 kills performance???

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }

            try
            {
                myConnection.Open();
                SqlR1 = SqlCmd1.ExecuteReader();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("DB Open2 Exception occured: " + ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }

            string FleURL;
            string FleName;
            
            richTextBox1.AppendText("Started downloading all large pictures \r");
            Application.DoEvents();

            while (SqlR1.Read())
            {
                FleURL = (SqlR1["high_pic"].ToString());
                FleName = ("C:\\userfolder\\ICECAT\\Generated\\Large\\" + SqlR1["ProductID"].ToString() + ".jpg" );
                try
                {
                    DownloadICEcatFile(FleURL, txtB_User.Text, txtB_Pass.Text, FleName);
                }
                catch (Exception ex)
                {
                    richTextBox2.AppendText("SQL Loop large pics exception occured: " + ex.Message + " \r");
                    richTextBox2.AppendText("Proces info URL: " + FleURL + "\r");
                    richTextBox2.AppendText("Proces info File: " + FleName + "\r");
                }
                Application.DoEvents();
             }

             try
             {
                 SqlR1.Close();
                 myConnection.Close();
             }
             catch (Exception ex)
             {
                 richTextBox2.AppendText("Exception occured: " + ex.Message + "\r");
             }
             richTextBox1.AppendText("Download large pictures done \r");
        }

        private void DownloadLowPictures()
        {
            timer1.Enabled = false;
            int ErrCnt;
            ErrCnt = 0;

            SqlConnection myConnection = new SqlConnection("user id=sa;" +
                                       "password=%kg77hB;server=172.16.0.63;" +
                                       "Trusted_Connection=yes;" +
                                       "database=ICECAT; " +
                                        "connection timeout=30");

            SqlDataReader SqlR1 = null;
            SqlCommand SqlCmd1 = new SqlCommand("Select distinct sku as ProductID, LowPic as low_pic from Haalnuweg_Jaap", myConnection);
            // top 1 kills performance???

            try
            {
                myConnection.Close();
            }
            catch
            {
                // Just means connection was left open
            }

            try
            {
                myConnection.Open();
                SqlR1 = SqlCmd1.ExecuteReader();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("Exception occured: " + ex.Message + " \r");
                ErrCnt = ErrCnt + 1;
            }

            string FleURL;
            string FleName;

            richTextBox1.AppendText("Started downloading all small pictures \r");
            Application.DoEvents();

            while (SqlR1.Read())
            {
                FleURL = (SqlR1["low_pic"].ToString());
                FleName = ("C:\\userfolder\\ICECAT\\Generated\\Small\\" + SqlR1["ProductID"].ToString() + ".jpg");
                try
                {
                    DownloadICEcatFile(FleURL, txtB_User.Text, txtB_Pass.Text, FleName);
                }
                catch (Exception ex)
                {
                    richTextBox2.AppendText("SQL Loop Small pics exception occured: " + ex.Message + " \r");
                    richTextBox2.AppendText("Proces info URL: " + FleURL + "\r");
                    richTextBox2.AppendText("Proces info File: " + FleName + "\r");
                }
                Application.DoEvents();
            }

            try
            {
                SqlR1.Close();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText("Exception occured: " + ex.Message + "\r");
            }
            richTextBox1.AppendText("Download small pictures done \r");
        }


        private void button7_Click(object sender, EventArgs e)
        {
            DownloadHighPictures();
            DownloadPictures();            
            DownloadLowPictures();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            CleanFullImportTable();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled != false) { timer1.Enabled = false; }
            if (timer_FullImport.Enabled != false) { timer_FullImport.Enabled = false; }

            GlobalClass.GlobalVar = "0";
            richTextBox1.AppendText("Timers disabled! \r");
            richTextBox2.AppendText("Timers disabled! \r");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (DownloadICEcatFileFullNL() > 0)
            {
                richTextBox2.AppendText("Stopped importing NL FULL table!");
                richTextBox2.AppendText("You can retry the same step");
            }
            else
            {
                ProcessXMLFullFileNL();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (DownloadICEcatFileFullEN() > 0)
            {
                richTextBox2.AppendText("Stopped importing EN FULL table!");
                richTextBox2.AppendText("You can retry the same step");
            }
            else
            {
                ProcessXMLFullFileEN();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Process_IceFullTable_AfterImport();
        }


    }
}