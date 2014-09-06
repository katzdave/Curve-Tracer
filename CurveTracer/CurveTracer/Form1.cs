using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurveTracer
{
    public partial class Form1 : Form
    {
        double[] Voltages = { 1, 5, 10, 20 };
        double[] Currents = { 1, 10, 100, 1000 };
        double MaxCurrent = 5;
        int Precision = 256;
        int Ameter = 1024;
        int Offset = 0; //12
        String CsvName = "temp";
        String RName = "jlab.R";
        String PdfName = "temp.pdf";
        String Params = "params.csv";


        public Form1()
        {
            InitializeComponent();
            //MessageBox.Show(System.Environment.CurrentDirectory);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                panel1.Show();
                panel4.Show();
                panel6.Show();
                panel7.Show();
                panel8.Show();
                label1.Show();
                label4.Show();
                label13.Show();
            }
            else
            {
                panel1.Hide();
                panel4.Hide();
                panel6.Hide();
                panel7.Hide();
                panel8.Hide();
                label1.Hide();
                label4.Hide();
                label13.Hide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Tracing...";
            button1.Enabled = false;
            bool[] VCVS_v = { radioButton5.Checked,
                                    radioButton6.Checked,
                                    radioButton7.Checked,
                                    radioButton8.Checked };

            bool[] VCVS_i = { radioButton9.Checked,
                                    radioButton10.Checked,
                                    radioButton11.Checked,
                                    radioButton12.Checked };

            bool[] VCCS_i = { radioButton1.Checked,
                                    radioButton2.Checked,
                                    radioButton3.Checked,
                                    radioButton4.Checked };

            bool[] VCCS_v = { radioButton13.Checked,
                                    radioButton14.Checked,
                                    radioButton15.Checked,
                                    radioButton16.Checked };

            int VCVS_a = Convert.ToInt32(VCVS_v[1] || VCVS_v[3]);
            int VCVS_b = Convert.ToInt32(VCVS_v[2] || VCVS_v[3]);
            int VCVS_c = Convert.ToInt32(VCVS_i[1] || VCVS_i[3]);
            int VCVS_d = Convert.ToInt32(VCVS_i[2] || VCVS_i[3]);

            int VCCS_a = Convert.ToInt32(VCCS_i[1] || VCCS_i[3]);
            int VCCS_b = Convert.ToInt32(VCCS_i[2] || VCCS_i[3]);
            int VCCS_c = Convert.ToInt32(VCCS_v[1] || VCCS_v[3]);
            int VCCS_d = Convert.ToInt32(VCCS_v[2] || VCCS_v[3]);

            double VCVS_maxv = Voltages[VCVS_a + 2*VCVS_b];
            double VCVS_maxi = Currents[VCVS_c + 2*VCVS_d];

            double VCCS_maxi = Currents[VCCS_a + 2*VCCS_b];
            double VCCS_maxv = Voltages[VCCS_a + 2*VCCS_d];

            //Current circuit restriction
            VCCS_maxi = MaxCurrent;

            bool isThreeTerminal = checkBox1.Checked;

            var VCVS_allPoints = new List<int>();
            var VCCS_allPoints = new List<int>();

            double VCVS_start = Convert.ToDouble(textBox1.Text);
            double VCVS_stop = Convert.ToDouble(textBox2.Text);
            int VCVS_npoints = Convert.ToInt16(textBox3.Text);
            double VCVS_step = (VCVS_stop - VCVS_start) / (VCVS_npoints - 1);

            if ((VCVS_start < (-1 * VCVS_maxv)) || (VCVS_stop > VCVS_maxv)
                || (VCVS_stop < (-1 * VCVS_maxv)) || (VCVS_start > VCVS_maxv))
            {
                MessageBox.Show("Error invalid VCVS start or stop values");
                button1.Text = "Trace!";
                button1.Enabled = true;
                return;
            }

            for (int i = 0; i < VCVS_npoints; i++)
            {
                double d = VCVS_start + i * VCVS_step;
                d = (d + VCVS_maxv) * (Precision - 1) / (2 * VCVS_maxv);
                VCVS_allPoints.Add(Convert.ToInt32(d));
            }
            VCVS_allPoints = VCVS_allPoints.Distinct().ToList();

            //2 Terminal sweep!!
            if (!isThreeTerminal)
            {
                VCCS_allPoints.Add(128);
            }
            else
            {
                double VCCS_start = Convert.ToDouble(textBox6.Text);
                double VCCS_stop = Convert.ToDouble(textBox5.Text);
                int VCCS_npoints = Convert.ToInt16(textBox4.Text);
                double VCCS_step = (VCCS_stop - VCCS_start) / (VCCS_npoints - 1);

                if ((VCCS_start < (-1 * VCCS_maxi)) || (VCCS_stop > VCCS_maxi)
                    || (VCVS_stop < (-1 * VCCS_maxi)) || (VCCS_start > VCCS_maxi))
                {
                    MessageBox.Show("Error invalid VCCS start or stop values");
                    button1.Text = "Trace!";
                    button1.Enabled = true;
                    return;
                }

                for (int i = 0; i < VCCS_npoints; i++)
                {
                    double d = VCCS_start + i * VCCS_step;
                    d = (d + VCCS_maxi) * (Precision - 1) / (2 * VCCS_maxi);
                    VCCS_allPoints.Add(Convert.ToInt32(d));
                }
                VCCS_allPoints = VCCS_allPoints.Distinct().ToList();
            }

            // Send startup message
            String startup = VCVS_a.ToString() + VCVS_b.ToString() + VCVS_c.ToString() + VCVS_d.ToString()
                + VCCS_a.ToString() + VCCS_b.ToString() + VCCS_c.ToString() + VCCS_d.ToString();

            string[] txtList = System.IO.Directory.GetFiles(Environment.CurrentDirectory, "*.csv");
            foreach (var v in txtList)
            {
                System.IO.File.Delete(v);
            }

            using (SerialPort port = new SerialPort("COM15", 9600))
            {
                port.Open();
                port.Write("s" + startup);
                Console.WriteLine(startup);
                String ack = port.ReadLine();
                Console.WriteLine(ack);

                int csvnum = 0;
                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter(Params))
                {
                    foreach (var i in VCCS_allPoints)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            var q = Precision - VCVS_allPoints.ElementAt(0) - 1;
                            port.Write("q" + q.ToString() + " " + i.ToString());
                            String garbage = port.ReadLine();
                        }

                        double current = (i - Precision / 2 - Offset) * VCCS_maxi / (Precision / 2);
                        if (radioButton17.Checked)
                        {
                            file1.WriteLine(Math.Round(current, 2).ToString() + " mA");
                        }
                        else
                        {
                            file1.WriteLine(Math.Round(current, 2).ToString() + " V");
                        }
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(CsvName + csvnum.ToString() + ".csv"))
                        {
                            var number = 0;
                            foreach (var p in VCVS_allPoints)
                            {
                                var q = Precision - p - 1;
                                port.Write("q" + q.ToString() + " " + i.ToString());
                                String vals = port.ReadLine();
                                String[] s = vals.Split(' ');

                                double vcvsCurrent = (Convert.ToDouble(s[0]) - Ameter / 2 - Offset) * VCVS_maxi / (Ameter / 2);
                                double vcvsVoltage = (p - Precision / 2 - Offset) * VCVS_maxv / (Precision / 2);
                                if (number++ > 0)
                                {
                                    file.WriteLine(vcvsVoltage.ToString() + "," + vcvsCurrent.ToString());
                                }

                                Console.WriteLine(p.ToString() + "," + vals);
                            }
                        }
                        csvnum++;
                    }
                    port.Write("q" + (Precision / 2).ToString() + " " + (Precision / 2).ToString());
                }
            }

            System.IO.File.Delete(PdfName);
            System.Diagnostics.Process.Start(RName);
            while (!System.IO.File.Exists(PdfName))
            {
                ;
            }
            System.Diagnostics.Process.Start(PdfName);

            button1.Text = "Trace!";
            button1.Enabled = true;
        }
    }
}
