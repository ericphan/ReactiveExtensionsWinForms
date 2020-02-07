using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RxWinFormsSample
{
    public partial class Form1 : Form
    {
        IObservable<string> _filtersChanged;
        IObservable<bool> _checkBox2Changed;

        IObservable<string> _usageCollector;


        public Form1()
        {
            InitializeComponent();

            var checkbox1CheckChanges = Observable.FromEventPattern(
                ev => checkBox1.CheckedChanged += ev,
                ev => checkBox1.CheckedChanged -= ev
                );



            var checkBox1Changed = Observable.FromEventPattern(ev => checkBox1.CheckedChanged += ev, ev => checkBox1.CheckedChanged -= ev)
                .Select(ev => ((CheckBox)ev.Sender).Checked)
                .StartWith(false);
            _checkBox2Changed = Observable.FromEventPattern(ev => checkBox2.CheckedChanged += ev, ev => checkBox2.CheckedChanged -= ev).Select(ev => ((CheckBox)ev.Sender).Checked).StartWith(false);
            var checkBox3Changed = Observable.FromEventPattern(ev => checkBox3.CheckedChanged += ev, ev => checkBox3.CheckedChanged -= ev).Select(ev => ((CheckBox)ev.Sender).Checked).StartWith(false);
            var checkBox4Changed = Observable.FromEventPattern(ev => checkBox4.CheckedChanged += ev, ev => checkBox4.CheckedChanged -= ev).Select(ev => ((CheckBox)ev.Sender).Checked).StartWith(false);
            var searchTextChanged = Observable.FromEventPattern(ev => txtSearch.TextChanged += ev, ev => checkBox4.TextChanged -= ev).Select(ev => ((TextBox)ev.Sender).Text).StartWith(string.Empty);

            _filtersChanged = Observable.CombineLatest(
                searchTextChanged, 
                checkBox1Changed, 
                _checkBox2Changed, 
                checkBox3Changed, 
                checkBox4Changed, 
                (text, c1, c2, c3, c4) =>
                {
                    return $"Keyword: {text}, Filter 1: {c1}, Filter 2: {c2}, Filter 3: {c3}, Filter 4: {c4}";
                })
            .Throttle(TimeSpan.FromSeconds(.5))
            ;

            _usageCollector = Observable.Merge(
                checkBox1Changed.Select(_ => "Checkbox 1"),
                _checkBox2Changed.Select(_ => "Checkbox 2"),
                checkBox3Changed.Select(_ => "Checkbox 3"),
                checkBox4Changed.Select(_ => "Checkbox 4"),
                searchTextChanged.Select(_ => "Search Textbox"),
                Observable.FromEventPattern(ev => btnSearch.Click += ev, ev => btnSearch.Click -= ev).Select(_ => "Search Button"));

        }

        private void buttonBackgroundJob_Click(object sender, EventArgs e)
        {
            var o = Observable.StartAsync(async () =>
            {
                txtResults.Text = "I'm doing something";
                progressBar1.Style = ProgressBarStyle.Marquee;
                return Task.Delay(3000);
            });

            o.Subscribe(_ =>
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                txtResults.Text += Environment.NewLine + "I'm done";
            }); 
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // Handle filter changes to trigger search
            _filtersChanged
                .ObserveOn(this)
                .Subscribe(options =>
            {
                txtResults.Text += options + Environment.NewLine;
            }, err =>
            {
                txtResults.Text += "Error" + Environment.NewLine;
            });



            //  Handle Enabling and Disabling of Filter Checkboxes
            _checkBox2Changed.ObserveOn(this).Subscribe(@checked =>
            {
                checkBox3.Enabled = @checked;
                checkBox4.Enabled = @checked;
            });


            // Collect usage stats
            _usageCollector.Timestamp()
                .Select(x => $"{x.Timestamp}: {x.Value}")
                .Buffer(TimeSpan.FromSeconds(5))
                .Where(usage => usage.Any())
                .ObserveOn(this)
                .Subscribe(senders =>
                {
                    txtResults.Text += "Sending usage stats [" + string.Join(",", senders) + "]" + Environment.NewLine;
                });
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // Trigger search

            // Enable disable stuff

            //fs
        }
    }
}
