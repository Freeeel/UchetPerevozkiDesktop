using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UchetPerevozki.Windows
{
    /// <summary>
    /// Логика взаимодействия для ReportsDetailsWindow.xaml
    /// </summary>
    public partial class ReportsDetailsWindow : Window
    {
        public ReportsDetailsWindow(ReportResponse report)
        {
            InitializeComponent();
            DisplayReportDetails(report);
        }
        private void DisplayReportDetails(ReportResponse report)
        {
            if (report != null)
            {
                IdTB.Text = report.Id.ToString();
                DateCreateTB.Text = report.report_date_time.ToString();
                DepartureTB.Text = report.point_departure;
                TypeDepartureTB.Text = report.type_point_departure;
                DestinationTB.Text = report.point_destination;
                TypeDestinationTB.Text = report.type_point_destination;
                SenderTB.Text = report.sender;
                RecipientTB.Text = report.recipient;
                TransporterTB.Text = report.user_full_name;
                ViewTB.Text = report.view_wood;
                LenghtTB.Text = report.length_wood.ToString();
                VolumeTB.Text = report.volume_wood.ToString();
                AssortementTB.Text = report.assortment_wood_type;
                SortTB.Text = report.variety_wood_type;
            }
        }
    }
 }
