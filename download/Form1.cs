using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;

namespace download
{
    public partial class download : Form
    {
        public download()
        {
            InitializeComponent();
        }

        System.Threading.SynchronizationContext m_SyncContext = null;
        private int _iTaskThis;
        private List<KeyValuePair<string, string>> _tasks;

        void TaskMain(object state)
        {
            const int iTaskSize = 50;
            for (int i = 0; i < iTaskSize; i++)
            {
                ThreadPool.QueueUserWorkItem(TaskWork, i);
            }
            while (true)
            {
                ThreadRunForMethodOne("This is task main");
                Thread.Sleep(1000);
            }
        }

        void TaskWork(object state)
        {
            while (true)
            {
                var text = string.Format("This is task {0}", state);
                ThreadRunForMethodOne(text);
                Thread.Sleep(1000);
            }
        }

        private void download_Load(object sender, EventArgs e)
        {
            var filename = "default.xml";
            if (File.Exists(filename))
            {
                tbFileName.Text = filename;
            }

            var lines = new string[20];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = "text " + i.ToString();
            }
            tbLog.Lines = lines;

            ThreadPool.SetMinThreads(5, 5);
            ThreadPool.SetMaxThreads(50, 50);
            m_SyncContext = SynchronizationContext.Current;
        }

        private void ThreadRunForMethodOne(object state)
        {
            m_SyncContext.Post(SendOrPostCallback, state);
        }

        void SendOrPostCallback(object state)
        {
            tbLog.AppendText((string)state + "\r\n");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var filename = tbFileName.Text;
            if (!File.Exists(filename))
            {
                MessageBox.Show("file not exist");
                return;
            }

            var doc = new XmlDocument();
            doc.Load(filename);

            _tasks = new List<KeyValuePair<string, string>>();
            foreach (var node in doc.DocumentElement)
            {
                if (node is XmlElement)
                {
                    var element = node as XmlElement;
                    _tasks.Add(new KeyValuePair<string, string>(element.GetAttribute("name"), element.GetAttribute("uri")));
                }
            }

            if (_tasks.Count > 0)
            {
                var btn = sender as Button;
                btn.Enabled = false;
                _iTaskThis = 0;
                ThreadPool.QueueUserWorkItem(TaskMain);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
