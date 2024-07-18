using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using ListViewEx;

using MC02Handler;

namespace EA_DB_Editor
{
    public partial class Form1 : Form
    {
        public List<Field> lMappedFields = new List<Field>();
        public List<MaddenTable> lMappedTables = new List<MaddenTable>();
        public List<View> lMappedViews = new List<View>();
        public MaddenDatabase maddenDB = null;
        public bool bConfigRead = false;
        public View currentView = null;
        static public bool mc02Recalc = false;
        public static Form1 MainForm;
        public static bool PreseasonScheduleEdit = false;

        public static Form1 CreateForm(AppDomain appDomain)
        {
            var form = (Form1)appDomain.CreateInstanceAndUnwrap(typeof(Form1).Assembly.FullName, typeof(Form1).FullName);
            return form;
        }

        public string FilePath { get; private set; }

        public void OpenDynastyFile(Guid correlationId, byte[] fileBytes)
        {
            var fileName = correlationId.ToString();
            File.WriteAllBytes(fileName, fileBytes);
            this.FilePath = Path.Combine(Environment.CurrentDirectory, fileName);

            Cursor.Current = Cursors.WaitCursor;
            maddenDB = new MaddenDatabase(fileName);

            // walked each table and field and add in the mapped elements
            foreach (MaddenTable mt in maddenDB.lTables)
            {
                MaddenTable mtmapped = MaddenTable.FindTable(lMappedTables, mt.Table.TableName);
                mt.Abbreviation = mt.Table.TableName;
                if (mtmapped != null)
                    mt.Name = mtmapped.Name;

                foreach (Field f in mt.lFields)
                {
                    Field fmapped = Field.FindField(lMappedFields, f.name);
                    f.Abbreviation = f.name;
                    if (fmapped != null)
                        f.Name = fmapped.Name;
                }
            }

            this.Text = maddenDB.realfileName.Substring(maddenDB.realfileName.LastIndexOf('\\') + 1);
            Cursor.Current = Cursors.Default;
        }


        public Form1()
        {
            MainForm = this;
            InitializeComponent();

            View.viewChanged = viewChange;
            View.getMappedField = getMappedField;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();
            filterToolStripMenuItem.Enabled = false;
            massToolStripMenuItem.Enabled = false;
            selectedToolStripMenuItem.Enabled = false;
            allVisibleToolStripMenuItem.Enabled = false;
            asNewItemsToolStripMenuItem.Enabled = false;
            overwriteExistingToolStripMenuItem.Enabled = false;
        }

        ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
        }

        public void ReadXMLConfig(string configfile)
        {
            Field field = null;
            MaddenTable table = null;
            View view = null;
            string Path = "\\";


            lMappedFields.Clear();
            lMappedTables.Clear();
            lMappedViews.Clear();

            XmlTextReader reader = new XmlTextReader(configfile);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        #region map open elements
                        if (Path == "\\xml\\" && reader.Name == "Field")
                            field = new Field();

                        if (Path == "\\xml\\" && reader.Name == "Table")
                            table = new MaddenTable();

                        if (Path == "\\xml\\" && reader.Name == "View")
                        {
                            view = new View();
                        }

                        if (reader.Name == "Formulas")
                        {
                            field.Formulas = Field.Formula.ReadFormulas(reader, Path + "Formulas\\");
                            break;
                        }
                        #endregion

                        Path += reader.Name + "\\";
                        break;

                    case XmlNodeType.Text:

                        #region map main entries
                        if (Path.EndsWith("Main\\Size\\Width\\"))
                            this.Size = new Size(Convert.ToInt32(reader.Value), this.Size.Height);

                        if (Path.EndsWith("Main\\Size\\Height\\"))
                            this.Size = new Size(this.Size.Width, Convert.ToInt32(reader.Value));
                        #endregion

                        #region map field entries
                        if (Path.EndsWith("Field\\Abbreviation\\"))
                            field.Abbreviation = reader.Value;

                        if (Path.EndsWith("Field\\Name\\"))
                            field.Name = reader.Value;

                        if (Path.EndsWith("Field\\ControlType\\"))
                        {
                            field.ControlType = reader.Value;
                            switch (reader.Value)
                            {
                                case "TextBox": field.EditControl = new TextBox(); break;
                                case "ComboBox": field.EditControl = CreateComboBox(); break;
                                case "CheckBox": field.EditControl = new CheckBox(); break;
                                case "Calculated": field.EditControl = new Label(); break;
                                case "AdjustedComboBox": field.EditControl = CreateComboBox(); break;
                                case "MappedComboBox": field.EditControl = CreateComboBox(); break;
                                case "TimeOfDayInMinutes": field.EditControl = new TextBox(); break;
                            }
                            if (field.EditControl != null)
                                field.EditControl.Visible = false;
                        }

                        if (Path.EndsWith("Field\\ControlItemCount\\"))
                            field.ControlItems = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("Field\\ControlItem\\"))
                        {
                            switch (field.ControlType)
                            {
                                case "ComboBox":
                                case "AdjustedComboBox":
                                case "MappedComboBox":
                                    ((ComboBox)field.EditControl).Items.Add(reader.Value);
                                    break;
                            }
                        }

                        if (Path.EndsWith("Field\\ControlLocked\\"))
                            field.ControlLocked = Convert.ToBoolean(reader.Value);

                        if (Path.EndsWith("Field\\ControlLink\\Table\\"))
                            field.ControlLink = reader.Value;

                        if (Path.EndsWith("Field\\ControlLink\\IndexField\\"))
                            field.ControlIF = reader.Value;

                        if (Path.EndsWith("Field\\ControlLink\\ReferenceField\\"))
                            field.ControlRF = reader.Value;

                        if (Path.EndsWith("Field\\ControlLink\\ReferenceField2\\"))
                            field.ControlRF2 = reader.Value;

                        if (Path.EndsWith("Field\\ControlLink\\Min\\"))
                            field.Min = Convert.ToDouble(reader.Value);

                        if (Path.EndsWith("Field\\ControlLink\\Max\\"))
                            field.Max = Convert.ToDouble(reader.Value);

                        if (Path.EndsWith("Field\\Offset\\"))
                            field.Offset = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("Field\\Description\\"))
                            field.Description = reader.Value;

                        if (Path.EndsWith("Field\\Type\\"))
                        {
                            switch (reader.Value)
                            {
                                case "uint": field.type = (ulong)Field.FieldType.tdbUInt; break;
                                case "sint": field.type = (ulong)Field.FieldType.tdbSInt; break;
                                case "string": field.type = (ulong)Field.FieldType.tdbString; break;
                                case "float": field.type = (ulong)Field.FieldType.tdbFloat; break;
                                case "binary": field.type = (ulong)Field.FieldType.tdbBinary; break;
                            }
                        }
                        #endregion

                        #region map table entries
                        if (Path.EndsWith("Table\\Abbreviation\\"))
                            table.Abbreviation = reader.Value;

                        if (Path.EndsWith("Table\\Name\\"))
                            table.Name = reader.Value;
                        #endregion

                        #region map view entries
                        if (Path.EndsWith("View\\Name\\"))
                            view.Name = reader.Value;

                        if (Path.EndsWith("View\\Type\\"))
                            view.Type = reader.Value;

                        if (Path.EndsWith("View\\Source\\Type\\"))
                            view.SourceType = reader.Value;

                        if (Path.EndsWith("View\\Source\\Name\\"))
                            view.SourceName = reader.Value;

                        if (Path.EndsWith("View\\Position\\X\\"))
                            view.Position_x = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("View\\Position\\Y\\"))
                            view.Position_y = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("View\\Position\\Z\\"))
                            view.Position_z = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("View\\Size\\Width\\"))
                            view.Size_width = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("View\\Size\\Height\\"))
                            view.Size_height = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("View\\ChildCount\\"))
                            view.ChildCount = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("View\\FieldCount\\"))
                            view.ChildFieldCount = Convert.ToInt32(reader.Value);

                        if (Path.EndsWith("View\\Child\\"))
                            view.ChildViews.Add(reader.Value);

                        if (Path.EndsWith("View\\Field\\"))
                            view.ChildFields.Add(reader.Value);
                        #endregion
                        break;

                    case XmlNodeType.EndElement:
                        #region map close elements
                        if (Path == "\\xml\\Field\\" && reader.Name == "Field")
                            lMappedFields.Add(field);

                        if (Path == "\\xml\\Table\\" && reader.Name == "Table")
                            lMappedTables.Add(table);

                        if (Path == "\\xml\\View\\" && reader.Name == "View")
                            lMappedViews.Add(view);
                        #endregion

                        try
                        {
                            Path = Path.Remove(Path.LastIndexOf(reader.Name + "\\"));
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("XML closing element not found: " + reader.Name + ", " + reader.LineNumber, "Error in XML config");
                            throw (e);
                        }
                        break;
                }
            }
            reader.Close();
            return;
        }
        public void PostProcessMaps()
        {
            int i, j;

            #region field post processing
            #region cross lnk combo boxes where appropriate
            for (i = 0; i < lMappedFields.Count; i++)
            {
                if (lMappedFields[i].ControlLink != "" && lMappedFields[i].ControlType == "ComboBox")
                {
                    ((ComboBox)lMappedFields[i].EditControl).Items.Clear();
                    lMappedFields[i].KeyToIndexMappings.Clear();

                    #region get the real table
                    MaddenTable mt = MaddenTable.FindTable(lMappedTables, lMappedFields[i].ControlLink);
                    if (mt == null)
                    {
                        MessageBox.Show("Table " + lMappedFields[i].ControlLink + "not found in Field->Control for field " + lMappedFields[i].Abbreviation, "Error in xml config");
                        continue;
                    }
                    mt = maddenDB[mt.Abbreviation];
                    #endregion

                    for (j = 0; j < mt.lRecords.Count; j++)
                    {
                        #region look up index & reference fields
                        Field fi = Field.FindField(lMappedFields, lMappedFields[i].ControlIF);
                        Field fr = Field.FindField(lMappedFields, lMappedFields[i].ControlRF);
                        Field fr2 = null;

                        if (lMappedFields[i].ControlRF2 != "")
                            fr2 = Field.FindField(lMappedFields, lMappedFields[i].ControlRF2);

                        if (fi == null)
                        {
                            MessageBox.Show("Field " + lMappedFields[i].Abbreviation + ": Control Link index field not found in table: " + lMappedFields[i].ControlIF, "Error in xml config");
                            continue;
                        }
                        if (fr == null)
                        {
                            MessageBox.Show("Field " + lMappedFields[i].Abbreviation + ": Control Link reference field not found in table: " + lMappedFields[i].ControlRF, "Error in xml config");
                            continue;
                        }
                        #endregion
                        #region add the mapping to the combobox for the field
                        //string value = "(" + mt.lRecords[j][fi.Abbreviation] + ") " + mt.lRecords[j][fr.Abbreviation];
                        string value = mt.lRecords[j][fi.Abbreviation] + ": " + mt.lRecords[j][fr.Abbreviation];
                        if (fr2 != null)
                            value += " " + mt.lRecords[j][fr2.Abbreviation];

                        RefObj rf = new RefObj(mt.lRecords[j][fi.Abbreviation], value);

                        ((ComboBox)lMappedFields[i].EditControl).Items.Add(rf);
                        if (lMappedFields[i].KeyToIndexMappings.ContainsKey(rf.key) == false)
                        {
                            lMappedFields[i].KeyToIndexMappings.Add(rf.key, ((ComboBox)lMappedFields[i].EditControl).Items.Count - 1);
                        }
                        #endregion
                    }
                }
            }
            #endregion
            #region update column fields with real field data
            foreach (View view in lMappedViews)
            {
                if (view.Type != "Grid") continue;

                MaddenTable mt = MaddenTable.FindTable(lMappedTables, view.SourceName);
                mt = maddenDB[mt.Abbreviation];

                foreach (ColumnHeader ch in ((ListView)view.DisplayControl).Columns)
                {
                    if (ch.Tag == null) continue;

                    Field colfield = (Field)ch.Tag;
                    Field f = Field.GetField(mt.lFields, colfield.Abbreviation);

                    if (f != null)
                    {
                        colfield.bits = f.bits;
                        colfield.name = f.name;
                        colfield.offset = f.offset;
                        colfield.type = f.type;
                    }
                }
            }
            #endregion
            #endregion
        }
        public void UpdateTableBoundViews()
        {
            foreach (View v in lMappedViews)
            {
                if (v.SourceType == "Table")
                {
                    MaddenTable mt = MaddenTable.FindTable(lMappedTables, v.SourceName);
                    if (v.Type == "Grid")
                        v.UpdateGridData(maddenDB[mt.Abbreviation]);
                }
            }
        }
        public void viewChange(View view)
        {
            if (view.lChildren.Count == 0)
                currentView = view;
            else
            {
                TabControl tab = (TabControl)view.DisplayControl;
                int sel = tab.SelectedIndex;
                tab.SelectedIndex = -1;
                tab.SelectedIndex = sel > -1 ? sel : 0;
            }
        }
        public Field getMappedField(string name)
        {
            return Field.FindField(lMappedFields, name);
        }

        public bool ChooseConfig()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "*.xml|*.xml|all|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select an XML config file to use...";

            if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog())
            {
                // clean up old controls first
                List<Control> remove = new List<Control>();
                foreach (Control c in this.Controls)
                {
                    if (c.GetType().ToString().EndsWith("TextBox") ||
                        c.GetType().ToString().EndsWith("ComboBox") ||
                        c.GetType().ToString().EndsWith("ListView") ||
                        c.GetType().ToString().EndsWith("TabControl"))
                        remove.Add(c);
                }
                foreach (Control c in remove)
                {
                    c.Controls.Clear();
                    this.Controls.Remove(c);
                }


                if (openFileDialog.FileName.ToUpper().Contains("Schedule.xml".ToUpper()))
                {
                    PreseasonScheduleEdit = true;
                }

                ReadXMLConfig(openFileDialog.FileName);

                if (!View.ProcessAllViewSettings(lMappedViews, lMappedFields))
                    this.Close();
                if (!View.SetViewChildren(lMappedViews, this))
                    this.Close();

                bConfigRead = true;
                return true;
            }
            return false;
        }
        public void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!bConfigRead)
            {
                MessageBox.Show("Please choose a config file before opening an EA file. You can use the option to generate one from a file if you need to.", "Alert!");
                if (!ChooseConfig())
                    return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"D:\OneDrive\ncaa";
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = ".*";
            openFileDialog.Filter = "(*.*)|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select MC02 file to open...";

            if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog())
            {
                Cursor.Current = Cursors.WaitCursor;
                maddenDB = new MaddenDatabase(openFileDialog.FileName);

                // walked each table and field and add in the mapped elements
                foreach (MaddenTable mt in maddenDB.lTables)
                {
                    MaddenTable mtmapped = MaddenTable.FindTable(lMappedTables, mt.Table.TableName);
                    mt.Abbreviation = mt.Table.TableName;
                    if (mtmapped != null)
                        mt.Name = mtmapped.Name;

                    foreach (Field f in mt.lFields)
                    {
                        Field fmapped = Field.FindField(lMappedFields, f.name);
                        f.Abbreviation = f.name;
                        if (fmapped != null)
                            f.Name = fmapped.Name;
                    }
                }

                this.Text = maddenDB.realfileName.Substring(maddenDB.realfileName.LastIndexOf('\\') + 1);

                int year = 0;

                if (int.TryParse(this.Text.Replace("DYNASTY-Y", "").Substring(0, 3), out year))
                {
                    DynastyYear = 2000 + year;
                }

                Cursor.Current = Cursors.Default;

                filterToolStripMenuItem.Enabled = true;
                massToolStripMenuItem.Enabled = true;
                selectedToolStripMenuItem.Enabled = true;
                allVisibleToolStripMenuItem.Enabled = true;
                asNewItemsToolStripMenuItem.Enabled = true;
                overwriteExistingToolStripMenuItem.Enabled = true;

                PostProcessMaps();
                UpdateTableBoundViews();
            }
        }
        public void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveFile();
            MessageBox.Show("Done");
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region open file
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "*.MC02|*.MC02|all|*.*";
            saveFileDialog.DefaultExt = ".MC02";
            saveFileDialog.Title = "Save as...";

            if (System.Windows.Forms.DialogResult.OK != saveFileDialog.ShowDialog())
                return;
            #endregion

            Cursor.Current = Cursors.WaitCursor;
            maddenDB.SaveAs(saveFileDialog.FileName);
            Cursor.Current = Cursors.Default;

            this.Text = maddenDB.realfileName.Substring(maddenDB.realfileName.LastIndexOf('\\') + 1);
            MessageBox.Show("Done");
        }

        public void SaveFile()
        {
            Cursor.Current = Cursors.WaitCursor;
            maddenDB.Save();
            Cursor.Current = Cursors.Default;
        }

        public void createConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = ".MC02";
            openFileDialog.Filter = "*.MC02|*.MC02|*.DB|*.DB|all|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select MC02 file to open...";

            if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog())
            {
                Cursor.Current = Cursors.WaitCursor;
                MaddenDatabase maddenDB2 = new MaddenDatabase(openFileDialog.FileName);
                Cursor.Current = Cursors.Default;

                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = ".xml";
                saveFileDialog.Filter = "*.xml|*.xml|all|*.*";
                saveFileDialog.Title = "Save xml config as...";

                if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog())
                {
                    Cursor.Current = Cursors.WaitCursor;

                    FileStream fs = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(fs);

                    sw.WriteLine("<xml>");
                    sw.WriteLine("");
                    sw.WriteLine("");

                    #region create the main tab view
                    sw.WriteLine("");
                    sw.WriteLine("<View>");
                    sw.WriteLine("\t<Name>MainTab</Name>");
                    sw.WriteLine("\t<Type>Tab</Type>");
                    sw.WriteLine("\t<Position>");
                    sw.WriteLine("\t\t<X>10</X>");
                    sw.WriteLine("\t\t<Y>30</Y>");
                    sw.WriteLine("\t\t<Z>0</Z>");
                    sw.WriteLine("\t</Position>");
                    sw.WriteLine("\t<Size>");
                    sw.WriteLine("\t\t<Width>800</Width>");
                    sw.WriteLine("\t\t<Height>340</Height>");
                    sw.WriteLine("\t</Size>");
                    #region add tables
                    foreach (MaddenTable mt in maddenDB2.lTables)
                        sw.WriteLine("\t<Child>" + mt.Table.TableName + "</Child>");
                    #endregion
                    sw.WriteLine("</View>");
                    sw.WriteLine("");
                    #endregion

                    foreach (MaddenTable mt in maddenDB2.lTables)
                    {
                        #region create a view
                        sw.WriteLine("");
                        sw.WriteLine("<View>");
                        sw.WriteLine("\t<Name>" + mt.Table.TableName + "</Name>");
                        sw.WriteLine("\t<Type>Grid</Type>");
                        sw.WriteLine("\t<Position>");
                        sw.WriteLine("\t\t<X>0</X>");
                        sw.WriteLine("\t\t<Y>0</Y>");
                        sw.WriteLine("\t\t<Z>0</Z>");
                        sw.WriteLine("\t</Position>");
                        sw.WriteLine("\t<Size>");
                        sw.WriteLine("\t\t<Width>200</Width>");
                        sw.WriteLine("\t\t<Height>100</Height>");
                        sw.WriteLine("\t</Size>");
                        sw.WriteLine("\t<Source>");
                        sw.WriteLine("\t\t<Type>Table</Type>");
                        sw.WriteLine("\t\t<Name>" + mt.Table.TableName + "</Name>");
                        sw.WriteLine("\t</Source>");
                        #region add fields
                        foreach (Field f in mt.lFields)
                            sw.WriteLine("\t<Field>" + f.name + "</Field>");
                        #endregion
                        sw.WriteLine("</View>");
                        sw.WriteLine("");
                        #endregion
                        #region create the table
                        sw.WriteLine("");
                        sw.WriteLine("<Table>");
                        sw.WriteLine("\t<Abbreviation>" + mt.Table.TableName + "</Abbreviation>");
                        sw.WriteLine("\t<Name></Name>");
                        sw.WriteLine("</Table>");
                        sw.WriteLine("");
                        #endregion
                        #region create the fields
                        foreach (Field f in mt.lFields)
                        {
                            string type = "";
                            switch (f.type)
                            {
                                case (ulong)Field.FieldType.tdbString: type = "string"; break;
                                case (ulong)Field.FieldType.tdbSInt: type = "sint"; break;
                                case (ulong)Field.FieldType.tdbUInt: type = "uint"; break;
                                case (ulong)Field.FieldType.tdbBinary: type = "binary"; break;
                                case (ulong)Field.FieldType.tdbFloat: type = "float"; break;
                            }
                            sw.WriteLine("<Field>");
                            sw.WriteLine("\t<Abbreviation>" + f.name + "</Abbreviation>");
                            sw.WriteLine("\t<Name></Name>");
                            sw.WriteLine("\t<ControlType>TextBox</ControlType>");
                            sw.WriteLine("\t<Type>" + type + "</Type>");
                            sw.WriteLine("</Field>");
                        }
                        #endregion
                    }

                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("</xml>");
                    sw.Flush();
                    sw.Close();
                    fs.Close();

                    Cursor.Current = Cursors.Default;

                }

            }
        }
        private void loadConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChooseConfig();
        }
        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterForm ff = new FilterForm(lMappedFields, lMappedTables, new List<View>() { currentView }, FilterForm.CBToUse.filter, "Filters");
            ff.ShowDialog();

            MaddenTable mt = MaddenTable.FindTable(lMappedTables, ff.view.SourceName);
            ff.view.UpdateGridData(maddenDB[mt.Abbreviation], ff.lFilters);
        }
        private void massToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterForm ff = new FilterForm(lMappedFields, lMappedTables, new List<View>() { currentView }, FilterForm.CBToUse.mass, "Mass Operations");
            ff.ShowDialog();

            foreach (ListViewItem lvi in ((ListView)ff.view.DisplayControl).Items)
            {
                foreach (FieldFilter mass in ff.lFilters)
                    mass.Process(lMappedFields, ((MaddenRecord)lvi.Tag));
            }

            MaddenTable mt = MaddenTable.FindTable(lMappedTables, ff.view.SourceName);
            ff.view.RefreshGridData(maddenDB[mt.Abbreviation]);
        }
        private void headerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<object> lo1 = new List<object>();
            List<object> lo2 = new List<object>();

            lo1.Add((object)"Header");
            lo1.Add((object)"Version");
            lo1.Add((object)"Uknown 1");
            lo1.Add((object)"DB Size");
            lo1.Add((object)"Zero");
            lo1.Add((object)"Table Count");
            lo1.Add((object)"Uknown 2");

            lo2.Add((object)maddenDB.dbFileInfo.header.ToString());
            lo2.Add((object)maddenDB.dbFileInfo.version.ToString());
            lo2.Add((object)maddenDB.dbFileInfo.unknown_1.ToString());
            lo2.Add((object)maddenDB.dbFileInfo.DBsize.ToString());
            lo2.Add((object)maddenDB.dbFileInfo.zero.ToString());
            lo2.Add((object)maddenDB.dbFileInfo.tableCount.ToString());
            lo2.Add((object)maddenDB.dbFileInfo.unknown_2.ToString());

            GenericList gl = new GenericList("", lo1, lo2);
            gl.Show();
        }
        private void selectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentView == null)
                return;

            if (currentView.Type.ToLower() == "grid" || currentView.Type.ToLower() == "list item")
            {	// make sure there is a selection
                if (((ListView)currentView.DisplayControl).SelectedItems.Count <= 0)
                    return;

                // now let's get the table
                MaddenTable mt = MaddenTable.FindTable(maddenDB.lTables, currentView.SourceName);

                // request the field to use as a key
                ChooseField cf = new ChooseField();
                cf.table = mt;
                cf.ShowDialog();

                if (cf.choosen == null)
                {
                    MessageBox.Show("No field choosen - canceling export");
                    return;
                }

                // now get the record selected
                MaddenRecord mr = (MaddenRecord)((ListView)currentView.DisplayControl).SelectedItems[0].Tag;

                #region now open the file dialog
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "*.csv|*.csv|all|*.*";
                saveFileDialog1.DefaultExt = ".csv";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.FileName = "";

                if (System.Windows.Forms.DialogResult.OK != saveFileDialog1.ShowDialog())
                    return;
                #endregion

                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);


                Cursor.Current = Cursors.WaitCursor;
                #region write the table name, fields, and record
                sw.WriteLine(mt.Abbreviation + "," + cf.choosen.name);

                foreach (Field f in mt.lFields)
                    sw.Write(f.name + ",");
                sw.WriteLine("");

                foreach (Field f in mt.lFields)
                    sw.Write(mr[f.name] + ",");
                sw.WriteLine("");

                sw.Flush();
                sw.Close();
                fs.Close();
                #endregion
                Cursor.Current = Cursors.Default;
            }
        }
        private void allVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentView == null)
                return;

            // now let's export the selection. get the table first
            MaddenTable mt = MaddenTable.FindTable(maddenDB.lTables, currentView.SourceName);

            // request the field to use as a key
            ChooseField cf = new ChooseField();
            cf.table = mt;
            cf.ShowDialog();

            if (cf.choosen == null)
            {
                MessageBox.Show("No field choosen - canceling export");
                return;
            }

            #region now open the file dialog
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "*.csv|*.csv|all|*.*";
            saveFileDialog1.DefaultExt = ".csv";
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.FileName = "";

            if (System.Windows.Forms.DialogResult.OK != saveFileDialog1.ShowDialog())
                return;
            #endregion

            FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            Cursor.Current = Cursors.WaitCursor;
            #region write the table name, fields, and records

            sw.WriteLine(mt.Abbreviation + "," + cf.choosen.name);

            foreach (Field f in mt.lFields)
                sw.Write(f.name + ",");
            sw.WriteLine("");

            foreach (ListViewItem lvi in ((ListView)currentView.DisplayControl).Items)
            {
                MaddenRecord mr = (MaddenRecord)lvi.Tag;

                foreach (Field f in mt.lFields)
                    sw.Write(mr[f.name] + ",");
                sw.WriteLine("");
            }

            sw.Flush();
            sw.Close();
            fs.Close();
            #endregion
            Cursor.Current = Cursors.Default;
        }
        private void asNewItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region open file
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "*.csv|*.csv|all|*.*";
            openFileDialog1.DefaultExt = ".csv";
            openFileDialog1.Multiselect = false;

            if (System.Windows.Forms.DialogResult.OK != openFileDialog1.ShowDialog())
                return;
            #endregion

            Cursor.Current = Cursors.WaitCursor;

            // should only ever have 1 file
            for (int iFile = 0; iFile < openFileDialog1.FileNames.Length; iFile++)
            {
                FileStream fs = new FileStream(openFileDialog1.FileNames[iFile], FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                #region read table & headers then check their validity
                string[] header = sr.ReadLine().Split(new char[] { ',' });
                if (header.Length < 2)
                {
                    MessageBox.Show("Corrupted header - should contain table name and key field");
                    sr.Close();
                    fs.Close();
                    return;
                }
                string table = header[0];
                string key = header[1];
                string[] sfields = sr.ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // get our table
                MaddenTable mt = MaddenTable.FindMaddenTable(maddenDB.lTables, table);
                if (mt == null)
                {
                    MessageBox.Show("The table this data is for cannot be found in this database");
                    sr.Close();
                    fs.Close();
                    return;
                }

                // make sure our table matches our view
                if (currentView.SourceName != mt.Name && currentView.SourceName != mt.Abbreviation)
                {
                    MessageBox.Show("The selected file contains data for a different table. This table: " + currentView.SourceName + " The file's: " + mt.Abbreviation);
                    sr.Close();
                    fs.Close();
                    return;
                }

                // make sure key exists
                Field keyf = Field.FindField(mt.lFields, key);
                if (keyf == null)
                {
                    MessageBox.Show("The key provided (" + key + ") does not exists in the " + mt.ToString() + " table");
                    sr.Close();
                    fs.Close();
                    return;
                }

                // check the import file only uses headers in our table
                for (int i = 0; i < sfields.Length; i++)
                {
                    if (mt.lFields.Find((a) => a.name == sfields[i]) == null)
                    {
                        MessageBox.Show("Table " + mt + " does not have a field named " + sfields[i] + " as listed in " + openFileDialog1.FileNames[iFile]);
                        sr.Close();
                        fs.Close();
                        return;
                    }
                }
                #endregion

                while (!sr.EndOfStream)
                {
                    #region read our data
                    string[] data = sr.ReadLine().Split(new char[] { ',' });

                    if (data.Length < sfields.Length)
                    {
                        MessageBox.Show(openFileDialog1.FileNames[iFile] + " does not have as many data entries as field entries listed");
                        sr.Close();
                        fs.Close();
                        return;
                    }
                    #endregion
                    #region create a new record to hold the data
                    MaddenRecord mr = new MaddenRecord(mt, mt.lFields);
                    for (int i = 0; i < sfields.Length; i++)
                    {
                        mr[sfields[i]] = data[i];
                    }
                    #endregion
                    #region now see if this record already exists
                    MaddenRecord exists = mt.lRecords.Find((a) => a[key] == mr[key]);
                    if (exists != null)
                    {
                        if (keyf.type != (ulong)Field.FieldType.tdbUInt && keyf.type != (ulong)Field.FieldType.tdbSInt)
                        {
                            MessageBox.Show("A record exists with the same data in field " + key + " which is not an integer type, therefore aborting the import");
                            sr.Close();
                            fs.Close();
                            return;
                        }

                        // now find the first number that isn't taken
                        List<int> ints = new List<int>();
                        foreach (MaddenRecord r in mt.lRecords)
                            ints.Add(Convert.ToInt32(r[key]));
                        ints.Sort();

                        int index = 0;
                        for (index = 0; index < mt.Table.maxrecords; index++)
                        {
                            if (index != ints[index])
                                break;
                        }

                        if (index >= mt.Table.maxrecords)
                        {
                            MessageBox.Show("The table " + mt.ToString() + " is full @ " + mt.Table.maxrecords.ToString() + " and therefore we cannot import a new entry; aborting");
                            sr.Close();
                            fs.Close();
                            return;
                        }

                        // set the value to an unused value for the key field
                        mr[key] = index.ToString();
                    }
                    #endregion
                    #region fell through, so we're adding the record
                    if (!mt.InsertRecord(mr))
                    {
                        MessageBox.Show("Table is full; cannot create a new record", "Error");
                        Cursor.Current = Cursors.Default;
                        return;
                    }
                    #endregion
                }

                PostProcessMaps();
                currentView.RefreshGridData(mt);

                sr.Close();
                fs.Close();

            }

            Cursor.Current = Cursors.Default;
        }
        private void overwriteExistingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region open file
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "*.csv|*.csv|all|*.*";
            openFileDialog1.DefaultExt = ".csv";
            openFileDialog1.Multiselect = false;

            if (System.Windows.Forms.DialogResult.OK != openFileDialog1.ShowDialog())
                return;
            #endregion

            Cursor.Current = Cursors.WaitCursor;

            // should only ever have 1 file
            for (int iFile = 0; iFile < openFileDialog1.FileNames.Length; iFile++)
            {
                FileStream fs = new FileStream(openFileDialog1.FileNames[iFile], FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                #region read table & headers then check their validity
                string[] header = sr.ReadLine().Split(new char[] { ',' });
                if (header.Length < 2)
                {
                    MessageBox.Show("Corrupted header - should contain table name and key field");
                    sr.Close();
                    fs.Close();
                    return;
                }
                string table = header[0];
                string key = header[1];
                string[] sfields = sr.ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // get our table
                MaddenTable mt = MaddenTable.FindMaddenTable(maddenDB.lTables, table);
                if (mt == null)
                {
                    MessageBox.Show("The table this data is for cannot be found in this database");
                    sr.Close();
                    fs.Close();
                    return;
                }

                // make sure our table matches our view
                if (currentView.SourceName != mt.Name && currentView.SourceName != mt.Abbreviation)
                {
                    MessageBox.Show("The selected file contains data for a different table. This table: " + currentView.SourceName + " The file's: " + mt.Abbreviation);
                    sr.Close();
                    fs.Close();
                    return;
                }

                // make sure key exists
                Field keyf = Field.FindField(mt.lFields, key);
                if (keyf == null)
                {
                    MessageBox.Show("The key provided (" + key + ") does not exists in the " + mt.ToString() + " table");
                    sr.Close();
                    fs.Close();
                    return;
                }

                // check the import file only uses headers in our table
                for (int i = 0; i < sfields.Length; i++)
                {
                    if (mt.lFields.Find((a) => a.name == sfields[i]) == null)
                    {
                        MessageBox.Show("Table " + mt + " does not have a field named " + sfields[i] + " as listed in " + openFileDialog1.FileNames[iFile]);
                        sr.Close();
                        fs.Close();
                        return;
                    }
                }
                #endregion

                int count = 1;
                while (!sr.EndOfStream)
                {
                    #region read our data
                    string[] data = sr.ReadLine().Split(new char[] { ',' });

                    if (data.Length < sfields.Length)
                    {
                        MessageBox.Show(openFileDialog1.FileNames[iFile] + " does not have as many data entries as field entries listed");
                        sr.Close();
                        fs.Close();
                        return;
                    }
                    #endregion
                    #region create a new record to hold the data
                    MaddenRecord mr = new MaddenRecord(mt, mt.lFields);
                    for (int i = 0; i < sfields.Length; i++)
                    {
                        mr[sfields[i]] = data[i];
                    }
                    #endregion
                    #region now copy over the existing record ( only the fields provided )
                    MaddenRecord exists = mt.lRecords.Find((a) => a[key] == mr[key]);
                    if (exists == null)
                    {
                        MessageBox.Show("Record # " + count + " was not found; skipping");
                        continue;
                    }

                    for (int i = 0; i < sfields.Length; i++)
                    {
                        exists[sfields[i]] = mr[sfields[i]];
                    }

                    count++;
                    #endregion
                }

                PostProcessMaps();
                currentView.RefreshGridData(mt);

                sr.Close();
                fs.Close();

            }

            Cursor.Current = Cursors.Default;
        }
        private void overwriteSelectedminusKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentView == null)
                return;

            if (currentView.Type.ToLower() != "grid" && currentView.Type.ToLower() != "list item")
                return;

            if (((ListView)currentView.DisplayControl).SelectedItems.Count <= 0)
                return;

            #region open file
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "*.csv|*.csv|all|*.*";
            openFileDialog1.DefaultExt = ".csv";
            openFileDialog1.Multiselect = false;

            if (System.Windows.Forms.DialogResult.OK != openFileDialog1.ShowDialog())
                return;
            #endregion

            Cursor.Current = Cursors.WaitCursor;

            // should only ever have 1 file
            for (int iFile = 0; iFile < openFileDialog1.FileNames.Length; iFile++)
            {
                FileStream fs = new FileStream(openFileDialog1.FileNames[iFile], FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                #region read table & headers then check their validity
                string[] header = sr.ReadLine().Split(new char[] { ',' });
                if (header.Length < 2)
                {
                    MessageBox.Show("Corrupted header - should contain table name and key field");
                    sr.Close();
                    fs.Close();
                    return;
                }
                string table = header[0];
                string key = header[1];
                string[] sfields = sr.ReadLine().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // get our table
                MaddenTable mt = MaddenTable.FindMaddenTable(maddenDB.lTables, table);
                if (mt == null)
                {
                    MessageBox.Show("The table this data is for cannot be found in this database");
                    sr.Close();
                    fs.Close();
                    return;
                }

                // make sure our table matches our view
                if (currentView.SourceName != mt.Name && currentView.SourceName != mt.Abbreviation)
                {
                    MessageBox.Show("The selected file contains data for a different table. This table: " + currentView.SourceName + " The file's: " + mt.Abbreviation);
                    sr.Close();
                    fs.Close();
                    return;
                }

                // make sure key exists
                Field keyf = Field.FindField(mt.lFields, key);
                if (keyf == null)
                {
                    MessageBox.Show("The key provided (" + key + ") does not exists in the " + mt.ToString() + " table");
                    sr.Close();
                    fs.Close();
                    return;
                }

                // check the import file only uses headers in our table
                for (int i = 0; i < sfields.Length; i++)
                {
                    if (mt.lFields.Find((a) => a.name == sfields[i]) == null)
                    {
                        MessageBox.Show("Table " + mt + " does not have a field named " + sfields[i] + " as listed in " + openFileDialog1.FileNames[iFile]);
                        sr.Close();
                        fs.Close();
                        return;
                    }
                }
                #endregion
                #region read our data
                string[] data = sr.ReadLine().Split(new char[] { ',' });

                if (data.Length < sfields.Length)
                {
                    MessageBox.Show(openFileDialog1.FileNames[iFile] + " does not have as many data entries as field entries listed");
                    sr.Close();
                    fs.Close();
                    return;
                }
                #endregion
                #region create a new record to hold the data
                MaddenRecord mr = new MaddenRecord(mt, mt.lFields);
                for (int i = 0; i < sfields.Length; i++)
                {
                    mr[sfields[i]] = data[i];
                }
                #endregion
                #region now copy over the existing record ( only the fields provided, minus the key )
                MaddenRecord selected = (MaddenRecord)((ListView)currentView.DisplayControl).SelectedItems[0].Tag;

                for (int i = 0; i < sfields.Length; i++)
                {
                    if (sfields[i] != key)
                        selected[sfields[i]] = mr[sfields[i]];
                }
                #endregion

                PostProcessMaps();
                currentView.RefreshGridData(mt);

                sr.Close();
                fs.Close();
            }

            Cursor.Current = Cursors.Default;
        }
        private void recalcMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            PostProcessMaps();
            MaddenTable mt = MaddenTable.FindTable(lMappedTables, currentView.SourceName);
            currentView.RefreshGridData(maddenDB[mt.Abbreviation]);
            Cursor.Current = Cursors.Default;
        }
        private void skipChecksumRecalcExNCAAOnOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mc02Recalc = !mc02Recalc;
            if (mc02Recalc)
                skipChecksumRecalcExNCAAOnOffToolStripMenuItem.Text = "MC02 Recalc: ON";
            else
                skipChecksumRecalcExNCAAOnOffToolStripMenuItem.Text = "MC02 Recalc: OFF";
        }
        private void copyConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region open src file
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "*.xml|*.xml|all|*.*";
            openFileDialog1.DefaultExt = ".xml";
            openFileDialog1.Title = "Select source config...";
            openFileDialog1.Multiselect = false;

            if (System.Windows.Forms.DialogResult.OK != openFileDialog1.ShowDialog())
                return;
            #endregion
            #region open dst file
            OpenFileDialog openFileDialog2 = new OpenFileDialog();

            openFileDialog2.Filter = "*.xml|*.xml|all|*.*";
            openFileDialog2.DefaultExt = ".xml";
            openFileDialog2.Title = "Select destination config...";
            openFileDialog2.Multiselect = false;

            if (System.Windows.Forms.DialogResult.OK != openFileDialog2.ShowDialog())
                return;
            #endregion

            List<XMLConfig> srcViews = new List<XMLConfig>();
            List<XMLConfig> srcTables = new List<XMLConfig>();
            List<XMLConfig> srcFields = new List<XMLConfig>();
            List<XMLConfig> dstViews = new List<XMLConfig>();
            List<XMLConfig> dstTables = new List<XMLConfig>();
            List<XMLConfig> dstFields = new List<XMLConfig>();

            Cursor.Current = Cursors.WaitCursor;

            XMLConfig.ReadXMLConfig(openFileDialog1.FileName, srcViews, srcTables, srcFields);
            XMLConfig.ReadXMLConfig(openFileDialog2.FileName, dstViews, dstTables, dstFields);

            ConfigCopySelection ccs = new ConfigCopySelection();
            ccs.lMappedFieldsDst = dstFields;
            ccs.lMappedFieldsSrc = srcFields;
            ccs.lMappedTablesDst = dstTables;
            ccs.lMappedTablesSrc = srcTables;

            ccs.ShowDialog();
            if (ccs.bCanceled)
                return;

            //XMLConfig.CopyMappedValues( srcTables, dstTables );
            //XMLConfig.CopyMappedValues( srcFields, dstFields );

            //dstViews.Sort( (a,b) => a.Name.CompareTo( b.Name ) );
            ccs.lMappedTablesRes.Sort((a, b) => a.Abbreviation.CompareTo(b.Abbreviation));
            ccs.lMappedFieldsRes.Sort((a, b) => a.Abbreviation.CompareTo(b.Abbreviation));
            XMLConfig.UseFriendlyNames(dstViews, ccs.lMappedTablesRes, ccs.lMappedFieldsRes);

            FileStream fs = new FileStream(openFileDialog2.FileName, FileMode.Truncate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(XMLConfig.WriteXMLConfig(dstViews, ccs.lMappedTablesRes, ccs.lMappedFieldsRes));
            sw.Flush();
            sw.Close();
            fs.Close();

            Cursor.Current = Cursors.Default;
        }
        private void refreshViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            MaddenTable mt = MaddenTable.FindTable(lMappedTables, currentView.SourceName);
            //currentView.RefreshGridData(maddenDB[mt.Abbreviation]);

            if (mt.Abbreviation == "SCHD" && Form1.PreseasonScheduleEdit)
            {
                currentView.SortForSchedule();
            }

            currentView.RefreshGridData(maddenDB[mt.Abbreviation]);
            Cursor.Current = Cursors.Default;
        }

        private void FixRecruiting(object sender, EventArgs e)
        {
            RecruitingFixup.Fix(true);
            Cursor.Current = Cursors.WaitCursor;
            MaddenTable mt = MaddenTable.FindTable(lMappedTables, currentView.SourceName);
            currentView.RefreshGridData(maddenDB[mt.Abbreviation]);
            Cursor.Current = Cursors.Default;
        }

        List<MaddenRecord> playersBuffed;

        private void preseasonFixToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LookForSchedules();
        }

        private static HashSet<long> foundInterConfGames = new HashSet<long>();
        private static bool Check(string homeStr, string awayStr)
        {
            if (homeStr == "Notre Dame" || awayStr == "Notre Dame")
                return false;

            var home = homeStr.GetHashCode();
            var away = awayStr.GetHashCode();

            var lv = Math.Min(home, away);
            lv = Math.Max(home, away) << 32;

            if (foundInterConfGames.Contains(lv))
                return false;

            foundInterConfGames.Add(lv);
            return true;
        }

        public static void LookForSchedules()
        {
            LookForSchedules(Form1.MainForm);
        }

        public static void LookForSchedules(Form1 form)
        {
            Dictionary<int, HashSet<string>> foundGames = new Dictionary<int, HashSet<string>>();
            Dictionary<int, List<string>> weeklyGames = new Dictionary<int, List<string>>();
            var teamScheduleTable = MaddenTable.FindTable(form.maddenDB.lTables, "TSCH");
            Dictionary<string, List<MaddenRecord>> schedules = new Dictionary<string, List<MaddenRecord>>();

            var allGamesAllTeams = teamScheduleTable.lRecords.GroupBy(mr => mr["TGID"].ToInt32()).ToDictionary(g => g.Key, g => g.ToArray());
            var P5Teams = allGamesAllTeams.Where(kvp => RecruitingFixup.IsP5(kvp.Key));
            StringBuilder sb = new StringBuilder();
            StringBuilder confGames = new StringBuilder();
            HashSet<string> matches = new HashSet<string>();
            foreach (var team in P5Teams)
            {
                var ooc = team.Value.Where(mr => mr["OGID"].ToInt32() != 1023).Where(mr => !RecruitingFixup.TeamAndConferences.TeamsInSameConference(team.Key, mr["OGID"].ToInt32())).ToArray();

                // no P5 opponents
                if (!ooc.Any(mr => RecruitingFixup.IsP5(mr["OGID"].ToInt32())))
                {
                    var fcsCount = ooc.Count(mr => mr["OGID"].ToInt32().IsFcsTeam());
                    sb.AppendLine(string.Format("TeamId:  {0}  - weeks  {1} - FCS={2}", RecruitingFixup.TeamNames[team.Key], string.Join(",", ooc.Select(mr => 1 + mr["SEWN"].ToInt32()).ToArray()), fcsCount));
                    sb.AppendLine();
                }
                else
                {
                    foreach (var game in ooc.Where(mr => mr["OGID"].ToInt32().IsP5()))
                    {
                        var home = game["THOA"].ToInt32() == 1 ? RecruitingFixup.TeamNames[game["TGID"].ToInt32()] : RecruitingFixup.TeamNames[game["OGID"].ToInt32()];
                        var away = game["THOA"].ToInt32() == 0 ? RecruitingFixup.TeamNames[game["TGID"].ToInt32()] : RecruitingFixup.TeamNames[game["OGID"].ToInt32()];
                        var match = string.Format("{0} at {1}", home, away);
                        if (matches.Contains(match) == false)
                        {
                            matches.Add(match);

                            if (Check(home, away))
                            {
                                confGames.AppendLine(match);
                            }
                        }
                    }
                }
            }

            File.WriteAllText("noP5OOC.txt", sb.ToString());
            File.WriteAllText("confGames.txt", confGames.ToString());


            for (int i = 0; i < teamScheduleTable.Table.currecords; i++)
            {
                var team = teamScheduleTable.lRecords[i]["TGID"];
                var opponent = teamScheduleTable.lRecords[i]["OGID"];

                if (RecruitingFixup.IsP5(opponent.ToInt32()) &&
                    RecruitingFixup.IsP5(team.ToInt32()) &&
                    !RecruitingFixup.TeamAndConferences.TeamsInSameConference(team.ToInt32(), opponent.ToInt32()) &&
                    !ScheduleFixup.IsNotreDameGame(team.ToInt32(), opponent.ToInt32()))
                {
                    var week = 1 + teamScheduleTable.lRecords[i]["SEWN"].ToInt32();

                    HashSet<string> fg = null;
                    List<string> wg = null;
                    if (!foundGames.TryGetValue(team.ToInt32(), out fg))
                    {
                        fg = new HashSet<string>();
                        foundGames.Add(team.ToInt32(), fg);
                    }

                    if (!weeklyGames.TryGetValue(week, out wg))
                    {
                        wg = new List<string>();
                        weeklyGames.Add(week, wg);
                    }

                    var desc = string.Format("Team {0} plays Team {1} in Week {2}", team, opponent, week);
                    fg.Add(desc);
                    wg.Add(desc);

                    TeamOOC teamOOC = null;
                    if (TeamOOC.TryGetValue(team.ToInt32(), out teamOOC) == false)
                    {
                        TeamOOC[team.ToInt32()] = teamOOC = new TeamOOC();
                    }

                    teamOOC.AddGame(opponent.ToInt32());
                }
                else if (RosterCopy.IsFcsTeam(opponent.ToInt32()))
                {
                    TeamOOC teamOOC = null;

                    if (TeamOOC.TryGetValue(team.ToInt32(), out teamOOC) == false)
                    {
                        TeamOOC[team.ToInt32()] = teamOOC = new TeamOOC();
                    }

                    teamOOC.HasFcsGame = true;
                }

                List<MaddenRecord> games = null;

                if (!schedules.TryGetValue(team, out games))
                {
                    games = new List<MaddenRecord>();
                    schedules.Add(team, games);
                }

                games.Add(teamScheduleTable.lRecords[i]);
            }

            // if we have 15 week schedule , we need this to be 15, 14 otherwise
            var badSchedule = schedules.Where(kvp => kvp.Value.Count != 15).Select(kvp => kvp.Key).ToArray();
            //var badSchedule = schedules.Where(kvp => kvp.Value.Count != 14).Select(kvp => kvp.Key).ToArray();
            if (badSchedule.Length > 0)
            {
                var teams = string.Join(" ; ", badSchedule);
                MessageBox.Show(teams);
            }

            sb = new StringBuilder();
            var list = foundGames.ToArray().Select(kvp => kvp.Value).Where(hs => hs.Count > 1).SelectMany(hs => hs.ToArray()).ToList();
            var flattenedFoundGames = new HashSet<string>(list);
            /*
            foreach (var key in weeklyGames.Keys.OrderBy(i => i))
            {
                sb.AppendLine("Week = " + key);
                foreach (var game in weeklyGames[key])
                {
                    if (flattenedFoundGames.Contains(game))
                    {
                        sb.Append("    ");
                        sb.AppendLine(game);
                    }
                }

                sb.AppendLine();
            }*/

            foreach (var key in foundGames.Keys)
            {
                // let teams play 1 a P5 opponent a year
                if (foundGames[key].Count <= 1)
                    continue;

                sb.AppendLine("Team=" + key);
                foreach (var value in foundGames[key])
                {
                    sb.AppendLine("       " + value);
                }
                sb.AppendLine();
            }

            File.WriteAllText("gamesToCheck.txt", sb.ToString());
        }

        public static Dictionary<int, TeamOOC> TeamOOC = new Dictionary<int, TeamOOC>();

        private void FixRankings()
        {
            FixRankings(Form1.MainForm);
        }

        private void FixRankings(Form1 form)
        {
            var recruitTable = MaddenTable.FindMaddenTable(form.maddenDB.lTables, "RCPT");

            foreach (var record in playersBuffed)
            {
                int? oldRecruitRank = null;
                int? positionRank = null;  //RCRK
                int? recruitRank = null;  //RCPR
                bool foundRank = false;
                var list = recruitTable.lRecords.Where(r => record["PRSI"].ToInt32() != r["PRSI"].ToInt32()).OrderBy(rec => rec["RCRK"].ToInt32()).ToArray();
                var ranks = list.Select(r => r["RCRK"].ToInt32()).OrderBy(i => i).ToArray();
                for (int i = 0; i < list.Length; i++)
                {
                    var comparand = list[i];
                    if (!foundRank && record["RCOV"].ToInt32() > comparand["RCOV"].ToInt32() && record["RPGP"] == comparand["RPGP"])
                    {
                        foundRank = true;
                        recruitRank = comparand["RCRK"].ToInt32();
                        oldRecruitRank = record["RCRK"].ToInt32();
                        record["RCRK"] = recruitRank.ToString();


                        positionRank = comparand["RCPR"].ToInt32();
                        record["RCPR"] = positionRank.ToString();
                        record["RCCB"] = comparand["RCCB"];

                    }

                    if (foundRank && oldRecruitRank.HasValue && comparand["RCRK"].ToInt32() < oldRecruitRank.Value)
                    {
                        comparand["RCRK"] = (comparand["RCRK"].ToInt32() + 1).ToString();

                        if (record["RPGP"] == comparand["RPGP"])
                        {
                            comparand["RCPR"] = (comparand["RCPR"].ToInt32() + 1).ToString();
                        }
                    }
                }
            }

            RefreshView();
            // MessageBox.Show("recruits reranked");
        }

        void RefreshView()
        {
            Cursor.Current = Cursors.WaitCursor;
            MaddenTable mt = MaddenTable.FindTable(lMappedTables, currentView.SourceName);
            currentView.RefreshGridData(maddenDB[mt.Abbreviation]);
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Set Last Weeks Polls to the Current week before fixing current week
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lastWeekPollToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // curent BCSRank is 62, Previous BCS Rank is 112
            // current MediaPollRanks is 64, PreviousMediaPoll is 99
            // current CoachesPollRank is 63, Previous Coaches Poll is 113
            var table = maddenDB.lTables[167];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                record.lEntries[99].Data = record.lEntries[64].Data;
                record.lEntries[112].Data = record.lEntries[62].Data;
                record.lEntries[113].Data = record.lEntries[63].Data;
            }

            RefreshView();
        }

        /// <summary>
        /// Fix poll points to reflect new order
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pollPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var table = maddenDB.lTables[167];

            // media poll points is 184
            var mediaPoints = table.lRecords.Select(record => Convert.ToInt32(record.lEntries[184].Data)).OrderByDescending(i => i).Take(126).ToArray();

            // cocahes poll points is 144
            var coachesPoints = table.lRecords.Select(record => Convert.ToInt32(record.lEntries[144].Data)).OrderByDescending(i => i).Take(126).ToArray();

            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];

                // get media rank
                var mediaRank = record.lEntries[64].Data.ToInt32();
                var coachRank = record.lEntries[63].Data.ToInt32();

                if (mediaRank > 0 && mediaRank < mediaPoints.Length)
                {
                    record.lEntries[184].Data = mediaPoints[mediaRank - 1].ToString();
                }

                if (coachRank > 0 && coachRank < coachesPoints.Length)
                {
                    record.lEntries[144].Data = coachesPoints[coachRank - 1].ToString();
                }
            }

            RefreshView();
        }

        private void copyOverRosterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RosterCopy.LoadSource(this, maddenDB, CopyAction.Roster);
        }

        private void copyOverCoachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RosterCopy.LoadSource(this, maddenDB, CopyAction.Coach);
        }

        private void copyOverHSAARosterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RosterCopy.LoadSource(this, maddenDB, CopyAction.HSAA);
        }

        private void fixRecruitingClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("IF YOU CREATED RECRUITS MAKE SURE TO UPDATE DontChange property:  make sure you ran with recruiting.xml and stay on the first tab.  then run reorder recruits", "", MessageBoxButtons.OKCancel);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //RecruitingFixup.ImproveLastGuyForEachPosition();
                playersBuffed = RecruitingFixup.PreseasonFixup();
                Cursor.Current = Cursors.WaitCursor;
                MaddenTable mt = MaddenTable.FindTable(lMappedTables, currentView.SourceName);
                currentView.RefreshGridData(maddenDB[mt.Abbreviation]);
                Cursor.Current = Cursors.Default;
                FixRankings();
                RecruitingFixup.Fix(false);
                //      MessageBox.Show("Press the test button to make sure recruits are reranked");
            }
        }

        private void fixScheduleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScheduleFixup.FixSchedule();
            ScheduleFixup.FixSchedule();
            //ScheduleFixup.SetSunBeltChampionship();
            ScheduleFixup.SetNeutralSiteLogos();
        }

        private void testCoachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO, teams that run option need Scramblers.  Pro teams should get Pocket Passers, everyone else can get Any
            /*
                      COCH - coach table

          CPID  - offensive playbook
          CDID  - defensive playbook
          0= 4-3
          1= 3-4
          2 = 3-3-5
          3 = 4-2-5
          4 = multi
          5 = 3-4 multi
          6 = 4-3 multi

          CQBT = QB tendency 
          CFUC = User Controlled Flag (1=User, 0 = CPU)
             * 
             * 
             * 
             * COCH = coach table
          CFUC = user controlled


           STTM = team table of sort 
          CFUC = user controlled
          */
        }

        static void SortTable(MaddenRecord[] sortedTable)
        {
            Dictionary<int, int> GroupRanks = new Dictionary<int, int>();

            // fix the arms on guys
            for (int i = 0; i < sortedTable.Length; i++)
            {

                var recruit = sortedTable[i];
                var group = recruit["RPGP"].ToInt32();

                if (!GroupRanks.ContainsKey(group))
                {
                    GroupRanks[group] = 1;
                }

                recruit["RCPR"] = GroupRanks[group].ToString();
                recruit["RCRK"] = (i + 1).ToString();
                recruit["RCCB"] = StarRating(i + 1, recruit["RCOV"].ToInt32());

                var arms = CAPGen.CAPGen.GetArms(group);

                if (arms != null)
                {
                    var size = recruit["BSAA"].ToInt32();
                    var def = recruit["BSAT"].ToInt32();
                    if (size < arms.Item1 && def < arms.Item2)
                    {
                        recruit["BSAA"] = arms.Item1.ToString();
                        recruit["BSAT"] = arms.Item2.ToString();
                    }
                }
                GroupRanks[group] += 1;
            }
        }

        static string StarRating(int rank, int rating)
        {
            // Prescout >= 80 is 5*
            // Prescout 70 - 79 is 4*
            if (rating >= 80 && rank <= 300)
                return "5";

            if (rating >= 70 && rank <= 400)
                return "4";

            if (rank <= 1000)
                return "3";
            else if (rank <= 1800)
                return "2";
            return "1";
        }

        private Dictionary<string, int> firstDict = null;
        private Dictionary<string, int> lastDict = null;

        private void reorderRecruitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");

            var sortedTable = recruitTable.lRecords.OrderBy(r => r["RCRK"].ToInt32()).Take(500).OrderByDescending(rec => RecruitOrderMetric(rec)).ToArray();
            SortTable(sortedTable);
            SortTable(recruitTable.lRecords.OrderBy(r => r["RCRK"].ToInt32()).ToArray());
            PositionNumbers.FixSizes(recruitTable.lRecords.Where(r => r["PRSI"].ToInt32() > RecruitingFixup.DontChange).ToList());

            // have less Balanced + Scrambling and more Pocket Passers
            var table = MaddenTable.FindTable(maddenDB.lTables, "RCPT");
            var qbs = table.lRecords.Where(mr => mr["RPGP"].ToInt32() == 0).GroupBy(mr => mr["PTEN"].ToInt32()).ToDictionary(g => g.Key, g => g.ToArray());

            // 1/3 of the slowest Balanced become Pocket Passers
            var slowestBalanced = qbs[1].OrderBy(mr => SpeedTest(mr)).Take(qbs[1].Length / 3).ToList();
            slowestBalanced.ForEach(mr => mr["PTEN"] = "0");

            // 1/4 of the slowest Scramblings become balanced
            var slowestScrambler = qbs[2].OrderBy(mr => SpeedTest(mr)).Take(qbs[2].Length / 4).ToList();
            slowestScrambler.ForEach(mr => mr["PTEN"] = "1");

            // have fun with names
            if (File.Exists("names.txt"))
            {
                var names = "names.txt".FromJsonFile<NamesFile>();

                if (firstDict == null)
                {
                    firstDict = names.First.ToDictionary(s => s, s => 0);
                    lastDict = names.Last.ToDictionary(s => s, s => 0);
                }

                var numRecruits = recruitTable.lRecords.Count;

                foreach (var recruit in recruitTable.lRecords.Where(r => r["PRSI"].ToInt32() > RecruitingFixup.DontChange))
                {
                    var position = recruit["PPOS"].ToInt32();

                    // helmet and face mask stay the same 
                    if (position != 19 && position != 20)
                    {
                        ChangeHelmet(recruit, position);
                    }

                    if (recruit["STAT"].ToInt32() == 10)
                    {
                        HandleHawaiian(recruit, names);
                        continue;
                    }

                    var face = Int32.Parse(recruit["PGHE"]);


                    // rebalance K/P 
                    if (face > 100 && (position == 20 || position == 19 || recruit["PLNA"].StartsWith("O'")) && NamesFile.GetInt(100) < 90)
                    {
                        face = 1 + NamesFile.GetInt(65);
                        recruit["PGHE"] = face.ToString();
                    }

                    string[] firstList = names.First;
                    string[] lastList = names.Last;

#if false
                    if (face < 100)
                    {
                        firstList = names.LDFN;
                        lastList = names.LDLN;
                    }
                    else if (face < 160)
                    {
                        firstList = names.MDFN;
                        lastList = names.MDLN;
                    }
#endif
                    if (face > 99)
                    {
                        ChangeName(face,
                            recruit,
                            firstDict,
                            lastDict,
                            names,
                            firstList,
                            lastList);
                    }
                }

                //                MessageBox.Show("First: " + maxFirst + "   Last: " + maxLast);
            }

            RefreshView();
        }

        static void ChangeHelmet(MaddenRecord recruit, int position)
        {
            switch (position)
            {
                case 0:
                    ChangeQbHelmet(recruit);
                    break;
                case 1:
                case 3:
                case 16:
                case 18:
                case 17:
                    ChangeSkillHelmet(recruit);
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    ChangeBigHelmet(recruit);
                    break;
                case 2:
                case 4:
                    ChangeHBackHelmet(recruit);
                    break;
                default:
                    break;
            }
        }

        static void ChangeHBackHelmet(MaddenRecord recruit)
        {
            var helmet = RandomHelmetType();
            int[] masks = null;

            switch (helmet)
            {
                case 3:
                    masks = new int[] { 7, 32 };
                    break;
                case 4:
                    masks = new int[] { 25, 29, 30, 39, 40 };
                    break;
                case 5:
                    masks = new int[] { 17, 33, 34, 35, 36 };
                    break;
                case 6:
                    masks = new int[] { 42, 44, 45, 19 };
                    break;
                default: break;
            }

            DoHelmetChange(recruit, helmet, masks);
        }

        static void ChangeSkillHelmet(MaddenRecord recruit)
        {
            var helmet = RandomHelmetType();
            int[] masks = null;

            switch (helmet)
            {
                case 3:
                    masks = new int[] { 32, 16, 7, 9, 10 };
                    break;
                case 4:
                    masks = new int[] { 25, 29, 30, 39, 40 };
                    break;
                case 5:
                    masks = new int[] { 17, 33, 35, 36, 21 };
                    break;
                case 6:
                    masks = new int[] { 44, 45, 19 };
                    break;
                default: break;
            }

            DoHelmetChange(recruit, helmet, masks);
        }

        static void ChangeBigHelmet(MaddenRecord recruit)
        {
            var helmet = RandomHelmetType();
            int[] masks = null;

            switch (helmet)
            {
                case 3:
                    masks = new int[] { 12, 13, 14, 20, 37 };
                    break;
                case 4:
                    masks = new int[] { 26, 27, 28 };
                    break;
                case 5:
                    masks = new int[] { 34, 41, 37, 36 };
                    break;
                case 6:
                    masks = new int[] { 42, 43, 19 };
                    break;
                default: break;
            }

            DoHelmetChange(recruit, helmet, masks);
        }

        static void ChangeQbHelmet(MaddenRecord recruit)
        {
            var helmet = RandomHelmetType();
            int[] masks = null;

            switch (helmet)
            {
                case 3:
                    masks = new int[] { 7, 9, 10, 11, 31 };
                    break;
                case 4:
                    masks = new int[] { 30, 39, 40 };
                    break;
                case 5:
                    masks = new int[] { 33, 17 };
                    break;
                case 6:
                    masks = new int[] { 6 };
                    break;
                default: break;
            }

            DoHelmetChange(recruit, helmet, masks);
        }

        static void DoHelmetChange(MaddenRecord recruit, int helmet, int[] facemasks)
        {
            var mask = facemasks[Rand100() % facemasks.Length];
            recruit["PHLM"] = helmet.ToString();
            recruit["PFMK"] = mask.ToString();
        }

        static int RandomHelmetType()
        {
            var value = Rand100();

            if (value < 20)
                return 3;

            if (value < 30)
                return 4;

            if (value < 90)
                return 5;

            return 6;
        }

        static int Rand100()
        {
            var guid = Guid.NewGuid().ToByteArray().Take(4).ToArray();
            var i = BitConverter.ToInt32(guid, 0);

            if (i < 0)
            {
                i &= 0x7fffffff;
            }

            return i % 100;
        }

        static int RAND(int range)
        {
            var guid = Guid.NewGuid().ToByteArray().Take(4).ToArray();
            var i = BitConverter.ToInt32(guid, 0);

            if (i < 0)
            {
                i &= 0x7fffffff;
            }

            return i % range;
        }




        static void ChangeName(int face, MaddenRecord recruit, Dictionary<string, int> firstDict, Dictionary<string, int> lastDict, NamesFile names, string[] firstList, string[] lastList)
        {
            // 20% of guys get a name change until we max out
            if (IsMatch(40) ||
            string.Equals(recruit["PFNA"], recruit["PLNA"]))
            {
                string first = null;

                while (true)
                {
                    first = names.GetName(firstList);
                    if (firstDict.TryGetValue(first, out var count))
                    {
                        if (count >= 1)// || (count == 1 && firstDict.Values.Any(v => v == 0)))
                        {
                            continue;
                        }

                        firstDict[first] = count + 1;
                        break;
                    }
                    else
                    {
                        firstDict[first] = 1;
                        break;
                    }
                }

                recruit["PFNA"] = first;

                if (IsMatch(50))
                {
                    string lastName = null;

                    while (true)
                    {
                        lastName = names.GetName(lastList);

                        if (lastDict.TryGetValue(lastName, out var count))
                        {
                            if (count >= 2)
                                continue;

                            lastDict[lastName] = count + 1;
                            break;
                        }
                        else
                        {
                            lastDict[lastName] = 1;
                        }
                    }



                    var suffix = Suffix();

                    if (IsMatch(5) &&
                        ((lastName.Length < 8 && suffix.Length == 2) ||
                        (lastName.Length < 7 && suffix.Length == 3)))
                    {
                        if (IsMatch(10))
                        {
                            lastName += " " + suffix;
                        }
                    }
                    recruit["PLNA"] = lastName;
                }
            }
        }

        public static void HandleHawaiian(MaddenRecord recruit, NamesFile names)
        {
            string face = null;
            string ln = null;
            string fn = null;
            // hawaiian demographic
            // 75% medium 105-159
            // 12% light 1-99
            // 12% dark  160-246
            // 67% last name change
            if (IsMatch(75))
            {
                // medium
                face = (105 + NamesFile.GetInt(45)).ToString();

                if (IsMatch(90)) ln = names.GetName(names.HILN);
                else ln = recruit["PLNA"];

                if (IsMatch(90))
                {
                    fn = names.GetName(names.HIFN);
                }
                else if (IsMatch(50))
                {
                    fn = recruit["PFNA"];
                }
                else
                {
                    fn = names.GetName(names.First);
                }
            }
            else if (IsMatch(50))
            {
                // light
                face = (1 + NamesFile.GetInt(99)).ToString();
            }
            else
            {
                // dark
                face = (160 + NamesFile.GetInt(85)).ToString();
                fn = names.GetName(names.First);

                if (IsMatch(75))
                {
                    ln = names.GetName(names.Last);
                }
                else
                {
                    ln = names.GetName(names.HILN);
                }
            }

            if (fn != null)
            {
                recruit["PFNA"] = fn;
            }

            if (ln != null)
            {
                recruit["PLNA"] = ln;
            }

            recruit["PGHE"] = face.ToString();
        }

        public static string Suffix()
        {
            var idx = NamesFile.GetInt() % Suffixes.Length;
            return Suffixes[idx];
        }

        public const string ThirdSuffix = "III";
        public const string JrSuffix = "Jr";
        public const string FourthSuffix = "IV";
        static string[] Suffixes = new string[] { ThirdSuffix, JrSuffix, FourthSuffix, "II" };

        public static bool IsMatch(int pct)
        {
            return pct > Rand100();
        }

        public class NamesFile
        {
            private string[] first;
            private string[] last;
            private List<string> hifn;
            private List<string> hiln;

            static RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

            public string[] First
            {
                get => first;
                set => first = new HashSet<string>(value.Where(s => s.Length <= 9)).ToArray();
            }

            public string[] Last
            {
                get => last;
                set => last = new HashSet<string>(value.Where(s => s.Length <= 12)).ToArray();
            }


            public List<string> HIFN
            {
                get => hifn;
                set => hifn = new HashSet<string>(value.Where(s => s.Length <= 9)).ToList();
            }

            public List<string> HILN
            {
                get => hiln;
                set => hiln = new HashSet<string>(value.Where(s => s.Length <= 12)).ToList();
            }

            public string[] LDFN { get; set; }
            public string[] LDLN { get; set; }
            public string[] MDFN { get; set; }
            public string[] MDLN { get; set; }


            public string GetName(string[] names)
            {
                var idx = GetInt() % names.Length;
                return names[idx];
            }

            public string GetName(List<string> names)
            {
                var idx = GetInt() % names.Count;
                var result = names[idx];
                names.RemoveAt(idx);
                return result;
            }

            public static int GetInt()
            {
                const int frame = 0x7FFFFFFF;
                var bytes = new byte[4];
                rand.GetBytes(bytes);
                return BitConverter.ToInt32(bytes, 0) & frame;
            }

            public static int GetInt(int lessThan)
            {
                return GetInt() % lessThan;
            }
        }

        private int RecruitOrderMetric(MaddenRecord r)
        {
            // position is P/K 15/16 or not a frosh
            var ovr = r["RCOV"].ToInt32();

            if (r["PYEA"].ToInt32() != 0 || r["RPGP"].ToInt32() == 15 || r["RPGP"].ToInt32() == 16)
                ovr -= 50;

            return ovr;
        }

        static int SpeedTest(MaddenRecord mr)
        {
            return mr["PACC"].ToInt32() + mr["PSPD"].ToInt32() + mr["PAGI"].ToInt32();
        }

        private void transferPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<MaddenTable> playerTables = new List<MaddenTable>();
            foreach (var table in maddenDB.lTables)
            {
                if (table.lFields.Any(f => f.Abbreviation == "PGID"))
                {
                    playerTables.Add(table);
                }
            }

            playerTables = playerTables.Where(mt => mt.lRecords.Count > 0 && mt.lFields.Any(f => f.Abbreviation == "TGID")).ToList();

            var playTable = playerTables.Where(t => t.Abbreviation == "PLAY").First().lRecords.GroupBy(mr => mr["TGID"].ToInt32()).Where(g => g.Count() > 0 && g.Count() < 70).ToDictionary(g => g.Key, g => g.ToArray());
            var keys = string.Join(",", playTable.Keys.ToArray().Select(i => i.ToString()).ToArray());
            MessageBox.Show(keys);
        }

        private bool Query(MaddenRecord mr)
        {
#if false
            var result = mr.Table.lFields.Where(f => f.Abbreviation == "TGID" || f.Name == "TGID").Any();

            if (result)
            {
                result &= mr["TGID"].ToInt32() == 22;
                result &= mr.lEntries.Any(data => data.field.type > 1 && data.Data.ToInt32() == 103);
            }

            return result;
#endif

            return false;
        }

        private void queryToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if false
            var table = MaddenTable.FindTable(maddenDB.lTables, "TEAM");
            var memphis = table.lRecords.Where(mr => mr["TGID"].ToInt32() == 48).FirstOrDefault();
            var field = memphis.lEntries.Where(le => le.Data == "Mem").ToList();
            field.ForEach(f => Debug.WriteLine( f.field.Abbreviation + ":" + f.field.Name));

            var t = MaddenTable.FindTable(maddenDB.lTables, "TEAM");

            var ohioState = t.lRecords.Where(r => r["TGID"] == "70").FirstOrDefault();
            var fields = ohioState.lEntries.Where(db => db.Data == "1").ToList();
            var bama = t.lRecords.Where(r => r["TGID"] == "3").FirstOrDefault();
            var fields2 = bama.lEntries.Where(db => db.Data == "2").ToList();
            var intersect = fields.Select(f => f.field.Name).Intersect(fields2.Select(f2 => f2.field.Name)).ToList();
            var intersect2 = fields.Select(f => f.field.Abbreviation).Intersect(fields2.Select(f2 => f2.field.Abbreviation)).ToList();
            Trace.WriteLine(t.Name);
            fields.ForEach(f => Trace.WriteLine(f.GetFieldName()));
            intersect.ForEach(i => Trace.WriteLine(i));
            intersect2.ForEach(i => Trace.WriteLine(i));
            var diff = ohioState.lEntries.Where(d => d.Data.ToInt32() == (bama[d.field.Abbreviation].ToInt32() - 1)).ToList();
            diff.ForEach(d => Trace.WriteLine(d.field.Name + " " + d.field.Abbreviation));
#endif
            // find high PR/KR yards to see what the bit field is
            var table = maddenDB.lTables[3];
            var rows = table.lRecords.Where(mr => mr["grky"].ToInt32() > 1000).Select(r => r["grky"]).ToList();
            rows.ForEach(r => Trace.WriteLine(r));
            rows = table.lRecords.Where(mr => mr["grpy"].ToInt32() > 1000).Select(r => r["grpy"]).ToList();
            rows.ForEach(r => Trace.WriteLine(r));

            table = MaddenTable.FindTable(maddenDB.lTables, "RCPT");
            var qbs = table.lRecords.Where(mr => mr["RPGP"].ToInt32() == 0).GroupBy(mr => mr["PTEN"].ToInt32()).ToDictionary(g => g.Key, g => g.ToArray());
            var topQB = table.lRecords.Where(mr => mr["RPGP"].ToInt32() == 0 && mr["RCRK"].ToInt32() < 500).GroupBy(mr => mr["PTEN"].ToInt32()).ToDictionary(g => g.Key, g => g.ToArray());
            var sb = new StringBuilder();
            sb.AppendLine("Pocket Passer:  " + qbs[0].Length);
            sb.AppendLine("Balanced:  " + qbs[1].Length);
            sb.AppendLine("Scrambling:  " + qbs[2].Length);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Pocket Passer:  " + topQB[0].Length);
            sb.AppendLine("Balanced:  " + topQB[1].Length);
            sb.AppendLine("Scrambling:  " + topQB[2].Length);
            MessageBox.Show(sb.ToString());
            PositionNumbers.Report(table);
        }

        private void headCoachModificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeamEntry entry = new TeamEntry();
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK && entry.TeamId.IsValidTeam())
            {
                // find the coach table
                var coch = MaddenTable.FindTable(maddenDB.lTables, "COCH");

                // find the head coach
                var coach = coch.lRecords.Where(mr => mr["TGID"].ToInt32() == entry.TeamId && mr["COPS"].ToInt32() == 0).FirstOrDefault();

                // find the oc
                var oc = coch.lRecords.Where(mr => mr["TGID"].ToInt32() == entry.TeamId && mr["COPS"].ToInt32() == 1).FirstOrDefault();

                // find the dc
                var dc = coch.lRecords.Where(mr => mr["TGID"].ToInt32() == entry.TeamId && mr["COPS"].ToInt32() == 2).FirstOrDefault();

                /*
                 * CPID = offensive playbook
                 * COTR = Off Run/Pass ratio
                 * COTA = off aggression
                 * COTS = off sub rate
                 * TNHS = no huddle style
                 * 
                 * CDID = defensive playbook
                 * CDTR = def run/pass ratio
                 * CDTS = def sub rate
                 * CDTA = def aggression
                 * */
                coach["CPID"] = oc["CPID"];
                coach["COTR"] = oc["COTR"];
                coach["COTA"] = oc["COTA"];
                coach["COTS"] = oc["COTS"];
                coach["TNHS"] = oc["TNHS"];

                coach["CDID"] = dc["CDID"];
                coach["CDTR"] = dc["CDTR"];
                coach["CDTS"] = dc["CDTS"];
                coach["CDTA"] = dc["CDTA"];
            }
        }

        private void assumeControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeamEntry entry = new TeamEntry();
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var sttm = MaddenTable.FindTable(maddenDB.lTables, "STTM");
                var coch = MaddenTable.FindTable(maddenDB.lTables, "COCH");

                // set the team as controlled
                var team = sttm.lRecords.Where(mr => mr["TGID"].ToInt32() == entry.TeamId).SingleOrDefault();

                // if the team is controlled, uncontrol, otherwise control
                var setting = "1";
                team["CFUC"] = setting;

                var coachPosition = entry.CoachPosition ?? 0;

                // find a coach as controlled
                var coach = coch.lRecords.Where(mr => mr["TGID"].ToInt32() == entry.TeamId && mr["COPS"].ToInt32() == coachPosition).FirstOrDefault();
                coach["CFUC"] = setting;
            }
        }

        private void queryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var tables = maddenDB.lTables.Where(t => t.lFields.Select(f => f.Abbreviation).Contains("CRBT")).ToList();
            tables.ForEach(t => Trace.WriteLine(t.Abbreviation));

            var coch = MaddenTable.FindTable(maddenDB.lTables, "COCH");
            foreach (var record in coch.lRecords)
            {
                record["CRBT"] = "50";
                record["COTY"] = "0";

                var playbook = record["CPID"].ToInt32();
                int style = 1;

                if (Style1Playbooks.Contains(playbook))
                {
                    style = 1;
                }
                else if (Style2Playbooks.Contains(playbook))
                {
                    style = 2;
                }
                else if (Style3Playbooks.Contains(playbook))
                {
                    style = 3;
                }
                else if (Style4Playbooks.Contains(playbook))
                {
                    style = 4;
                }

                record["COST"] = style.ToString();
                record["CDTY"] = "95";
            }
        }

        /* playbooks for 2013-2371
        private static HashSet<int> Style1Playbooks = new HashSet<int>(new[] { 174, 173, 135, 170, 169, 168, 167, 166, 164, 1, 2, 3, 4, 8, 9, 10, 14, 15, 16, 19, 20, 22, 23, 25, 29, 33, 36, 37, 42, 44, 45, 47, 49, 50, 54, 56, 57, 58, 61, 62, 63, 64, 65, 67, 68, 69, 70, 71, 72, 75, 79, 83, 85, 89, 90, 91, 92, 93, 94, 95, 97, 100, 102, 103, 107, 108, 112, 113, 115, 118, 130, 131, 133, 134 });
        private static HashSet<int> Style2Playbooks = new HashSet<int>(new[] { 5, 6, 11, 12, 13, 17, 18, 21, 24, 26, 27, 35, 39, 41, 46, 48, 53, 55, 60, 73, 76, 84, 88, 99, 101, 106, 109, 110, 129 });
        private static HashSet<int> Style3Playbooks = new HashSet<int>(new[] { 0, 7, 30, 59 });
        private static HashSet<int> Style4Playbooks = new HashSet<int>(new[] { 28, 31, 32, 34, 38, 40, 43, 51, 52, 66, 74, 77, 78, 80, 81, 82, 86, 87, 96, 98, 104, 105, 111, 114, 116, 117, 125, 132, 163, 165, 162 });
        */
        private static HashSet<int> Style1Playbooks = new HashSet<int>(new[] { 162, 163, 120, 135, 1, 2, 3, 4, 8, 9, 10, 14, 15, 16, 19, 20, 22, 23, 25, 29, 33, 36, 37, 42, 44, 45, 47, 49, 50, 54, 56, 57, 58, 61, 62, 63, 64, 65, 67, 68, 69, 70, 71, 72, 75, 79, 83, 85, 89, 90, 91, 92, 93, 94, 95, 97, 100, 102, 103, 107, 108, 112, 113, 115, 118, 130, 131, 133, 134 });
        private static HashSet<int> Style2Playbooks = new HashSet<int>(new[] { 5, 6, 11, 12, 13, 17, 18, 21, 24, 26, 27, 35, 39, 41, 46, 48, 53, 55, 60, 73, 76, 84, 88, 99, 101, 106, 109, 110, 129 });
        private static HashSet<int> Style3Playbooks = new HashSet<int>(new[] { 0, 7, 30, 59 });
        private static HashSet<int> Style4Playbooks = new HashSet<int>(new[] { 28, 31, 32, 34, 38, 40, 43, 51, 52, 66, 74, 77, 78, 80, 81, 82, 86, 87, 96, 98, 104, 105, 111, 114, 116, 117, 125, 132 });

        private void createCAPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CAPGen.CAPGen.GetRandomPlayer();
        }

        private void controlAllP5TeamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sttm = MaddenTable.FindTable(maddenDB.lTables, "STTM");
            var coch = MaddenTable.FindTable(maddenDB.lTables, "COCH");
            var teams = sttm.lRecords.Where(mr => (mr["TGID"] == "68" || mr["TGID"].ToInt32().IsP5()) && mr["TGID"].ToInt32() != 91).ToList();

            teams.ForEach(team =>
                {
                    var setting = (team["CFUC"].ToInt32() == 0 ? 1 : 0).ToString();
                    team["CFUC"] = setting;

                    // find a coach as controlled
                    var coach = coch.lRecords.Where(mr => mr["TGID"].ToInt32() == team["TGID"].ToInt32()).FirstOrDefault();
                    coach["CFUC"] = setting;
                });
        }

        private void preseasonScheduleFixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsEvenYear.HasValue == false)// && RecruitingFixup.AccTeams<16)
            {
                MessageBox.Show("Need to know if its a odd year or even year for ACC schedule");
                return;
            }

            ScheduleFixup.FixPreseasonSchedule();
        }

        private void readScheduleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsEvenYear.HasValue == false)// && RecruitingFixup.AccTeams < 16)
            {
                MessageBox.Show("Need to know if its a odd year or even year for ACC schedule");
                return;
            }

            ScheduleFixup.ReadSchedule();
        }

        public class TransferCandidate
        {
            public int Id { get; set; }
            public int OVR { get; set; }
            public int Year { get; set; }
            public string First { get; set; }
            public string Last { get; set; }
            public string Team { get; set; }
            public int TeamId { get; set; }

            public bool Redshirted { get; set; }

            public string State { get; set; }

            public int P5 => this.TeamId.IsP5() ? 1 : 2;

            public string Position { get; set; }

            public int PositionNumber { get; set; }

            public string ToCsvLine()
            {
                return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", Id, OVR, Position, First, Last, Team, TeamId, Year, State);
            }
        }

        public class TeamRosterFilled
        {
            public TeamRosterFilled(int id)
            {
                TeamId = id;
                Team = RecruitingFixup.TeamNames[id];
            }
            public int TeamId { get; set; }
            public string Team { get; set; }
            public bool[] Spots = new bool[70];
            public int Offset = 0;

            public List<int> NotFilled
            {
                get
                {
                    var result = new List<int>();
                    for (int i = 0; i < Spots.Length; i++)
                    {
                        if (Spots[i] == false)
                        {
                            result.Add(i + Offset);
                        }
                    }
                    return result;
                }
            }

            public string ToCsv()
            {
                var notFilled = NotFilled;
                return string.Format("{0},{1},{2},{3}", TeamId, notFilled.Count, Team, string.Join(",", notFilled));
            }
        }

        private static Dictionary<int, string> PlayerStates = new Dictionary<int, string>();

        private void DumpRosters()
        {
            Dictionary<int, List<TransferCandidate>> GetRosters()
            {
                return MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023)
                    .GroupBy(
                        mr => mr["TGID"].ToInt32(),
                        mr => new TransferCandidate
                        {
                            Id = mr["PGID"].ToInt32(),
                            OVR = mr["POVR"].ToInt32(),
                            Year = mr["PYEA"].ToInt32(),
                            First = mr["PFNA"],
                            Last = mr["PLNA"],
                            //                            Team = RecruitingFixup.TeamNames[mr["TGID"].ToInt32()],
                            TeamId = mr["TGID"].ToInt32(),
                            Redshirted = mr["PRSD"].ToInt32() == 2,
                            State = PlayerStates.TryGetValue(mr["RCHD"].ToInt32(), out var st) ? st : "unknown",
                            Position = mr["PPOS"].ToInt32().ToPositionName(),
                            PositionNumber = mr["PPOS"].ToInt32(),
                        })
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.PositionNumber).ThenByDescending(p => p.OVR).ThenByDescending(p => p.Year).ToList());
            }

            var allRosters = GetRosters();
            var dir = Directory.CreateDirectory("rosters");
            foreach (var kvp in allRosters)
            {
                var roster = new StringBuilder();
                kvp.Value.ForEach(p => roster.AppendLine(p.ToCsvLine()));

                var file = Path.Combine(dir.FullName, $"{kvp.Key}.csv");

                try
                {
                    File.WriteAllText(file, roster.ToString());
                }
                catch { }
            }
        }


        private void srTransferQBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PlayerStates.Count == 0)
            {
                var lines = File.ReadAllLines("cities.csv");

                foreach (var line in lines)
                {
                    var split = line.Split(',');
                    PlayerStates.Add(split[0].Trim().ToInt32(), split[2].Trim());
                }
            }

            /*
             * EASP = heisman watch?  (kemp, stanford, stephens,nichols, maxwell)
MCOV = media coverage
DCHT = depth chart  player id, team id, pos=0, depth =0
PLAY = player table

PGID - player id
TGID - team id
PFNA - first name 
PLNA - last name
PYEA - year (3) = senior
POVR = overall
PPOS = Position
             */
            Dictionary<int, TransferCandidate[]> GetPlayers(Func<int, bool> positionPredicate = null)
            {
                if (positionPredicate == null) positionPredicate = i => true;
                // QB depth chart
                return MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && positionPredicate(mr["PPOS"].ToInt32()))
                    .GroupBy(
                        mr => mr["TGID"].ToInt32(),
                        mr => new TransferCandidate
                        {
                            Id = mr["PGID"].ToInt32(),
                            OVR = mr["POVR"].ToInt32(),
                            Year = mr["PYEA"].ToInt32(),
                            First = mr["PFNA"],
                            Last = mr["PLNA"],
                            Team = RecruitingFixup.TeamNames[mr["TGID"].ToInt32()],
                            TeamId = mr["TGID"].ToInt32(),
                            Redshirted = mr["PRSD"].ToInt32() == 2,
                            State = PlayerStates.TryGetValue(mr["RCHD"].ToInt32(), out var st) ? st : "unknown",
                            Position = mr["PPOS"].ToInt32().ToPositionName(),
                        })
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.OVR).ThenBy(p => p.Year).ToArray());
            }

            DumpRosters();

            // each one with SR backup greater than 85
            // not Qbs, 3rd stringers
            var other = new StringBuilder();
            for (int i = 1; i <= 18; i++)
            {
                var otherPlayers = GetPlayers(pos => pos == i);
                var otherCandidates = otherPlayers.Values.SelectMany(p => p.Skip(2)).Where(p => p.OVR >= 85 && (p.Year == 3)).OrderByDescending(p => p.OVR).ToList();
                otherCandidates.ForEach(c => other.AppendLine(c.ToCsvLine()));
            }

            // g5 superstars, jr/sr above 95
            var g5stars = new StringBuilder();

            for (int i = 0; i <= 18; i++)
            {
                var otherPlayers = GetPlayers(pos => pos == i);
                var otherCandidates = otherPlayers.Values.SelectMany(p => p).Where(p => p.TeamId.IsG5() && p.OVR >= 95 && (p.Year >= 2)).OrderByDescending(p => p.OVR).ToList();
                otherCandidates.ForEach(c => g5stars.AppendLine(c.ToCsvLine()));
            }

            // QBs
            var players = GetPlayers(pos => pos == 0);
            var candidates = players.Values.SelectMany(p => p.Skip(1)).Where(p => p.OVR >= 85 && (p.Year == 3 || (p.Year == 2 && p.Redshirted))).OrderByDescending(p => p.OVR).ToList();
            var inNeed = players.Where(kvp => kvp.Key.IsP5OrND() && kvp.Value.First().OVR < 90).Select(kvp => kvp.Value.First().Team).ToList();
            var g5InNeed = players.Where(kvp => !kvp.Key.IsFcsTeam() && !kvp.Key.IsP5OrND() && kvp.Value.First().OVR < 85).Select(kvp => kvp.Value.First().Team).ToList();
            inNeed.AddRange(g5InNeed);

            StringBuilder sb = new StringBuilder();

            // write transfers
            candidates.ForEach(c => sb.AppendLine(c.ToCsvLine()));
            //inNeed.ForEach(c => sb.AppendLine(c));
            sb.AppendLine();

            // each teams QB depth chart
            foreach (var dc in players.Values.Where(tc => inNeed.Contains(tc.First().Team)).OrderBy(tc => tc.First().P5).ThenBy(tc => tc.First().OVR).ThenBy(tc => tc.First().Team))
            {
                sb.AppendLine(string.Empty);
                sb.AppendLine(string.Empty);
                foreach (var p in dc)
                {
                    sb.AppendLine(p.ToCsvLine());
                }
            }

            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);

            try
            {
                File.WriteAllText("transfercandidates.csv", sb.ToString());
                File.WriteAllText("transferPortal.csv", other.ToString());
                File.WriteAllText("g5stars.csv", g5stars.ToString());
            }
            catch { }

            Dictionary<int, TeamRosterFilled> spotsFilled = new Dictionary<int, TeamRosterFilled>();

            // all team ranges start at a multiple of 70 and go to a multiple of 70 -1 (e.g.  140-209)
            foreach (var player in MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023))
            {
                var team = player["TGID"].ToInt32();
                var pgid = player["PGID"].ToInt32();

                if (spotsFilled.ContainsKey(team) == false)
                {
                    spotsFilled[team] = new TeamRosterFilled(team);
                }

                spotsFilled[team].Spots[pgid % 70] = true;

                if (spotsFilled[team].Offset == 0)
                {
                    spotsFilled[team].Offset = (pgid / 70) * 70;
                }
            }

            sb = new StringBuilder();
            foreach (var value in spotsFilled.Values.OrderBy(v => v.Team))
            {
                sb.AppendLine(value.ToCsv());
            }

            try
            {
                File.WriteAllText("Roster.csv", sb.ToString());
            }
            catch { }

            var entry = new PlayerEntry();
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (entry.From == 999999)
                {
                    var lines = File.ReadAllLines("from.txt");
                    var offset = lines.Length / 2;

                    var fromLines = lines.Take(lines.Length / 2).ToArray();
                    var toLines = lines.Skip(lines.Length / 2).ToArray();

                    // check to make sure we don't have duplicates
                    bool CheckForUniqueness(string[] linesToCheck, string scenario)
                    {
                        var set = new HashSet<string>();

                        foreach (var line in lines)
                        {
                            if (!set.Add(line))
                            {
                                MessageBox.Show($"Duplicate value in {scenario}", line);
                                return false;
                            }
                        }

                        return true;
                    }

                    if (!CheckForUniqueness(fromLines, "from") || !CheckForUniqueness(toLines, "to"))
                    {
                        return;
                    }

                    for (int i = 0; i < offset; i++)
                    {
                        var from = Convert.ToInt32(lines[i]);
                        var to = Convert.ToInt32(lines[i + offset]);

                        var player = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["PGID"].ToInt32() == from).FirstOrDefault();

                        if (player != null)
                        {
                            player["PGID"] = to.ToString();
                            player["TGID"] = (to / 70).ToString();
                        }
                    }
                }
                else
                {
                    var player = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["PGID"].ToInt32() == entry.From).FirstOrDefault();

                    if (player != null)
                    {
                        player["PGID"] = entry.To.ToString();
                        player["TGID"] = (entry.To / 70).ToString();
                    }
                }
            }

#if false
            StringBuilder sb = new StringBuilder();

            // All back up QBs
            var dchtTbl = MaddenTable.FindTable(maddenDB.lTables, "DCHT").lRecords.Where(mr => mr["PPOS"].ToInt32() == 0 && mr["ddep"].ToInt32() > 0 && mr["TGID"] != "1023").ToArray();
            var dcht = new Dictionary<int, MaddenRecord>();
            foreach (var d in dchtTbl)
            {
                var playerId = d["PGID"].ToInt32();
                if (dcht.ContainsKey(playerId) == false)
                {
                    dcht.Add(playerId, d);
                }
            }

            // senior qbs over 90
            var players = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["POVR"].ToInt32() > 90 && mr["PYEA"].ToInt32() == 3);
            foreach (var player in players)
            {
                // find if the QB is a backup
                if (dcht.ContainsKey(player["PGID"].ToInt32()))
                {
                    sb.AppendLine(string.Format("{0} {1}, {2}", player["PFNA"], player["PLNA"], RecruitingFixup.TeamNames[player["TGID"].ToInt32()]));
                }
            }

            sb.AppendLine("Teams with QB needs");
            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);

            // sub 90 starters
            dcht = MaddenTable.FindTable(maddenDB.lTables, "DCHT").lRecords.Where(mr => mr["TGID"].ToInt32().IsP5OrND() && mr["PPOS"].ToInt32() == 0 && mr["ddep"].ToInt32() == 0 && mr["TGID"] != "1023").ToArray()
                .ToDictionary(mr => mr["PGID"].ToInt32());

            players = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["POVR"].ToInt32() < 90);
            foreach (var player in players)
            {
                // find if the QB is a backup
                if (dcht.ContainsKey(player["PGID"].ToInt32()))
                {
                    sb.AppendLine(string.Format("{0} {1}, {2}", player["PFNA"], player["PLNA"], RecruitingFixup.TeamNames[player["TGID"].ToInt32()]));
                }
            }

            File.WriteAllText("transfercandidates.txt", sb.ToString());
#endif
        }

        private static int StaffRating = 125;

        private void fireStaffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeamEntry entry = new TeamEntry();
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK && entry.TeamId.IsValidTeam())
            {
                var cprf = MaddenTable.FindTable(maddenDB.lTables, "CPRF");
                var coachTable = MaddenTable.FindTable(maddenDB.lTables, "COCH");

                Func<MaddenRecord, bool> match = input => true;

                if (entry.CoachPosition.HasValue)
                {
                    match = record => record["COPS"].ToInt32() == entry.CoachPosition.Value;
                }

                var coaches = coachTable.lRecords.Where(mr => mr["TGID"].ToInt32() == entry.TeamId && match(mr)).ToArray();

                var toSwap = cprf.lRecords.Where(mr => mr["JSCR"].ToInt32() == StaffRating).ToArray();


                foreach (var coach in coaches)
                {
                    // find the coach to be fired's record in CPRF table
                    var record = cprf.lRecords.Where(mr => mr["CCID"].ToInt32() == coach["CCID"].ToInt32()).FirstOrDefault();

                    // find a guy to swap with the same Postion (COPS)
                    var r = coachTable.lRecords.Where(mr => toSwap.Select(ts => ts["CCID"]).Contains(mr["CCID"]) && mr["COPS"] == coach["COPS"]).FirstOrDefault();

                    if (r != null)
                    {
                        var replacement = toSwap.Where(mr => mr["CCID"] == r["CCID"]).FirstOrDefault();
                        var jscp = replacement["JSCR"];
                        replacement["JSCR"] = record["JSCR"];
                        record["JSCR"] = jscp;
                        record["JSCP"] = "5";
                    }
                }

                StaffRating--;
            }
        }

        private void aCCScheduleForOddYearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsEvenYear = false;
        }

        private void aCCScheduleForEvenYearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsEvenYear = true;
        }

        private static int dynastyYear;
        public static bool? IsEvenYear = null;
        public static int DynastyYear
        {
            get
            {
                return dynastyYear;
            }

            set
            {
                dynastyYear = value;
                IsEvenYear = (value % 2) == 0;
            }
        }

        private void fixCoachOffensiveStyleToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void generateHomeRecordFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var file = "homediff.txt";
#if false
            CreateHomeFile(file);
#else
            // read and change the values
            var hr = File.ReadAllText(file).FromJson<HomeRecord[]>().ToDictionary(h => h.TeamId);
            var table = MaddenTable.FindTable(maddenDB.lTables, "TEAM");
            foreach (var mr in table.lRecords.Where(r => r["TGID"].ToInt32().IsValidTeam()))
            {
                var id = mr["TGID"].ToInt32();
                var record = hr[id];
                mr["TCHW"] = (mr["TCHW"].ToInt32() - record.Win).ToString();
                mr["TCHL"] = (mr["TCHL"].ToInt32() - record.Loss).ToString();
                mr["TCHT"] = (mr["TCHT"].ToInt32() - record.Tie).ToString();
                CreateHomeFile("updatedhomerecord.txt");
            }

#endif
        }

        private void CreateHomeFile(string file)
        {
            var teams = new List<HomeRecord>();
            var table = MaddenTable.FindTable(maddenDB.lTables, "TEAM");
            foreach (var mr in table.lRecords.Where(r => r["TGID"].ToInt32().IsValidTeam()))
            {
                teams.Add(new HomeRecord
                {
                    TeamId = mr["TGID"].ToInt32(),
                    Win = mr["TCHW"].ToInt32(),
                    Loss = mr["TCHL"].ToInt32(),
                    Tie = mr["TCHT"].ToInt32()
                });
            }

            using (var fs = File.Open(file, FileMode.Create, FileAccess.ReadWrite))
            {
                var js = new DataContractJsonSerializer(typeof(HomeRecord[]));
                js.WriteObject(fs, teams.ToArray());
            }
        }

        private void importRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void generateRecordsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // built in records are those with SEWN == 31 and RCDY==0
            // year == 0 is first year in the dynasty file
            // RBKS table
            // load the current file
            var file = @"D:\OneDrive\ncaa\Default NCAA Files\BaseSchoolRecords.txt";
            var records = File.ReadAllText(file).FromJson<TeamRecord[]>();
            var dict = records.ToDictionary(r => r.GetHashCode());

            var recordBook = MaddenTable.FindTable(maddenDB.lTables, "RBKS");
            foreach (var record in recordBook.lRecords)
            {
                var key = TeamRecord.CreateKey(record["RCDM"].ToInt32(), record["RCDI"].ToInt32(), record["RCDT"].ToInt32());
                var teamRecord = dict[key];
                var holder = record["RCDH"];
                int realYear = 0;

                // this is not a updated
                if (record["SEWN"].ToInt32() == 31)
                {
                    // career record has the format xx-yy
                    if (record["RCDT"].ToInt32() == 2)
                    {
                        var parts = holder.Substring(0, holder.IndexOf(' ')).Split('-');
                        if (parts.Last().Length == 4)
                        {
                            realYear = Convert.ToInt32(parts.Last());
                        }
                        else
                        {
                            realYear = Convert.ToInt32(parts.First().Substring(0, 2) + parts.Last());
                        }

                    }
                    /*                    else if (record["RCDV"].ToInt32() != teamRecord.Value)
                                        {
                                            realYear = 0;
                                        }*/
                    else
                    {
                        realYear = Convert.ToInt32(holder.Substring(0, holder.IndexOf(' ')));
                    }
                }
                else
                {
                    var firstSpace = holder.IndexOf(' ');
                    // year it happened
                    realYear = Convert.ToInt32(holder.Substring(0, firstSpace));

                    // who did it
                    teamRecord.Holder = holder.Substring(firstSpace + 1);
                    if (teamRecord.Holder[1] == '.')
                        teamRecord.Holder = teamRecord.Holder[0] + " " + teamRecord.Holder.Substring(2);

                    teamRecord.Value = record["RCDV"].ToInt32();
                    teamRecord.Opponent = record["RCDO"];
                    teamRecord.DynastyYear = record["RCDY"].ToInt32();
                }

                teamRecord.RealYear = realYear;
            }

            records.ToJsonFile(file);
        }

        private void refreshScheduleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            MaddenTable mt = MaddenTable.FindTable(lMappedTables, currentView.SourceName);
            currentView.RefreshGridData(maddenDB[mt.Abbreviation]);
            Cursor.Current = Cursors.Default;
        }

        private void playerNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PositionNumbers.Run();
        }

        private void fixG5SchedulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsEvenYear.HasValue == false)// && RecruitingFixup.AccTeams < 16)
            {
                MessageBox.Show("Need to know if its a odd year or even year for ACC schedule");
                return;
            }

            ScheduleFixup.ReadSchedule(true);
            ScheduleFixup.RanG5Fixup = true;
        }

        private void dumpTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // delete everything and create dir first
            var dir = Path.Combine(@".\", "tabledump");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            StringBuilder sb = null;
            Directory.CreateDirectory(dir);
            int idx = 0;

            foreach (var table in maddenDB.lTables)
            {
                var name = table.Abbreviation ?? table.Name;
                sb = new StringBuilder();
                var columns = table.lFields.Select(f => f.Abbreviation).ToArray();
                sb.AppendLine(string.Join(",", columns));

                foreach (var row in table.lRecords)
                {
                    sb.AppendLine(string.Join(",", columns.Select(c => row[c])));
                }

                try
                {
                    var sb2 = new StringBuilder();
                    sb2.AppendLine($"Name: {table.Name}");
                    sb2.AppendLine($"Abbreviation: {table.Abbreviation}");
                    sb2.AppendLine($"Max Count: {table.Table.maxrecords}");
                    File.WriteAllText(Path.Combine(dir, name + ".meta.txt"), sb2.ToString());
                }
                catch { }

                try
                {
                    File.WriteAllText(Path.Combine(dir, name + ".csv"), sb.ToString());
                }
                catch
                {
                    name = "t" + idx;
                    File.WriteAllText(Path.Combine(dir, name + ".csv"), sb.ToString());
                }
            }

            var scheduleTable = MaddenTable.FindTable(maddenDB.lTables, "SCHD").lRecords.Where(r => r["SEYR"].ToInt32() == 0);
            var list = new List<Dictionary<string, string>>();
            foreach (var record in scheduleTable)
            {
                list.Add(new Dictionary<string, string>()
                {
                    {"GATG", record["GATG"] },
                    {"GHTG", record["GHTG"] },
                    {"GTOD", record["GTOD"] },
                    {"SGNM", record["SGNM"] },
                    {"SEWN", record["SEWN"] },
                    {"SEWT", record["SEWT"] },
                    {"GDAT", record["GDAT"] },
                    {"GFFU", record["GFFU"] },
                    {"GMFX", record["GMFX"] },
                });
            }

            list.ToJsonFile(SCHDFILE);

            var teamScheduleTable = MaddenTable.FindTable(maddenDB.lTables, "TSCH");
            var teamTable = MaddenTable.FindTable(maddenDB.lTables, "TEAM");

            // first for each team build a dictionary of conference record
            var teamConfRecord = new Dictionary<int, ConferenceRecord>();
            foreach (var team in teamTable.lRecords)
            {
                var conferenceWin = team.lEntries[193].Data.ToInt32();
                var conferenceLoss = team.lEntries[181].Data.ToInt32();

                teamConfRecord[team.lEntries[40].Data.ToInt32()] = ConferenceRecord.Create(conferenceWin, conferenceLoss);
            }

            var teamConfOppRecord = new Dictionary<int, ConferenceRecord>();

            foreach (var record in teamScheduleTable.lRecords.Where(mr => mr.lEntries[4].Data.ToInt32() < 15))
            {
                // the team the schedule is for
                var teamId = record.lEntries[2].Data.ToInt32();

                // the opponent
                var oppId = record.lEntries[1].Data.ToInt32();

                // in the same conference
                if (RecruitingFixup.TeamAndConferences.TryGetValue(teamId, out var teamConfId) &&
                    RecruitingFixup.TeamAndConferences.TryGetValue(oppId, out var oppConfId) &&
                    teamConfId == oppConfId)
                {
                    if (teamConfOppRecord.TryGetValue(teamId, out var oppRecord))
                    {
                        var confRecord = teamConfRecord[oppId];
                        teamConfOppRecord[teamId].Add(confRecord);
                    }
                    else
                    {
                        teamConfOppRecord[teamId] = ConferenceRecord.Create(teamConfRecord[oppId]);
                    }
                }
            }

            var sorted = teamConfOppRecord.OrderBy(kvp => RecruitingFixup.TeamAndConferences[kvp.Key]).ThenByDescending(kvp => kvp.Value.WinPct);
            sb = new StringBuilder();
            foreach (var team in sorted)
            {
                sb.AppendLine($"{RecruitingFixup.TeamNames[team.Key]},{team.Value.WinPct},{team.Value.Win},{team.Value.Loss}");
            }

            File.WriteAllText(Path.Combine(dir, "Conference-Opp-Records.csv"), sb.ToString());
        }

        public class ConferenceRecord
        {
            public int Win { get; private set; }

            public int Loss { get; private set; }

            public int WinPct => (1000 * Win) / (Win + Loss);

            public static ConferenceRecord Create(int win, int loss) => new ConferenceRecord { Win = win, Loss = loss };

            public static ConferenceRecord Create(ConferenceRecord record) => new ConferenceRecord { Win = record.Win, Loss = record.Loss };

            public void Add(int win, int loss)
            {
                this.Win += win;
                this.Loss += loss;
            }

            public void Add(ConferenceRecord record)
            {
                this.Win += record.Win;
                this.Loss += record.Loss;
            }
        }

        const string SCHDFILE = "schedule.txt";

        private void importScheduleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var list = SCHDFILE.FromJsonFile<List<Dictionary<string, string>>>();
            var scheduleTable = MaddenTable.FindTable(maddenDB.lTables, "SCHD").lRecords.Where(r => r["SEYR"].ToInt32() == 0).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                foreach (var kvp in list[i])
                {
                    scheduleTable[i][kvp.Key] = kvp.Value;
                }
            }
        }

        private void bowlPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // set all game # to 127 in SGIN table
            var sginTable = MaddenTable.FindTable(maddenDB.lTables, "SGIN").lRecords;
            foreach (var row in sginTable)
            {
                row["SGNM"] = "127";
            }

            //RecruitingFixup.TeamAndConferences
            var bowlTable = MaddenTable.FindTable(maddenDB.lTables, "BOWL").lRecords
                .Where(mr => mr["SEWN"].ToInt32() > 16)
                .ToDictionary(
                mr => mr["BIDX"].ToInt32(),
                mr =>
                new
                {
                    Id = mr["BIDX"].ToInt32(),
                    Name = mr["BNME"],
                    GameNumber = mr["SGNM"].ToInt32(),
                    ConfId1 = mr["BCI1"].ToInt32(),
                    ConfId2 = mr["BCI2"].ToInt32(),
                    ConfRank1 = mr["BCR1"].ToInt32(),
                    ConfRank2 = mr["BCR2"].ToInt32(),
                });

            foreach (var b in AdditionalGameProvider.BowlIdToAddedGame)
            {
                bowlTable[b.Key] = new
                {
                    Id = b.Key,
                    Name = AdditionalGameProvider.BowlIdToName[b.Key],
                    GameNumber = b.Value,
                    ConfId1 = 0,
                    ConfId2 = 0,
                    ConfRank1 = 0,
                    ConfRank2 = 0,
                };
            }

            var big6Games = new HashSet<int>(new[] {
                AdditionalGameProvider.AddedGameToBowlId[AdditionalGameProvider.CFP5v12],
                AdditionalGameProvider.AddedGameToBowlId[AdditionalGameProvider.CFP6v11],
                AdditionalGameProvider.AddedGameToBowlId[AdditionalGameProvider.CFP7v10],
                AdditionalGameProvider.AddedGameToBowlId[AdditionalGameProvider.CFP8v9],
                25, 27, 28, 17, 12, 26, 39 });

            var schedules = MaddenTable.FindTable(maddenDB.lTables, "SCHD").lRecords
                .Where(mr => mr["SEWN"].ToInt32() > 16)
                .ToDictionary(mr => mr["SGNM"].ToInt32(),
                mr => new
                {
                    GameNumber = mr["SGNM"].ToInt32(),
                    WeekNumber = mr["SEWN"].ToInt32(),
                    Away = mr["GATG"].ToInt32(),
                    Home = mr["GHTG"].ToInt32(),
                });

            var matchups = new List<string>();
            matchups.Add(string.Join(",", "Game #", "Bowl", "", "Home", "Away", "", "Home", "Away"));
            matchups.Add(string.Join(",", "", "", "", "", "", "", "", ""));

            foreach (var game in big6Games)
            {
                var bowl = bowlTable[game];

                if (schedules.TryGetValue(bowl.GameNumber, out var scheduledGame))
                {
                    matchups.Add(string.Join(",", bowl.GameNumber, bowl.Name, string.Empty, RecruitingFixup.TeamNames[scheduledGame.Home], RecruitingFixup.TeamNames[scheduledGame.Away], "", "", ""));
                }
                else
                {
                    matchups.Add(string.Join(",", bowl.GameNumber, bowl.Name, "", "", "", "", "", ""));
                }
            }

            foreach (var game in bowlTable.Where(b => !big6Games.Contains(b.Key)).OrderBy(b => b.Value.GameNumber))
            {
                var bowl = game.Value;

                if (schedules.TryGetValue(bowl.GameNumber, out var scheduledGame))
                {

                    if (scheduledGame.Home == 1023) continue;

                    matchups.Add(string.Join(",", bowl.GameNumber, bowl.Name, string.Empty, RecruitingFixup.TeamNames[scheduledGame.Home], RecruitingFixup.TeamNames[scheduledGame.Away], "", "", ""));
                }
                else
                {
                    matchups.Add(string.Join(",", bowl.GameNumber, bowl.Name, "", "", "", "", "", ""));
                }
            }

            for (int j = 0; j < 15; j++)
            {
                matchups.Add(string.Join(",", "", "", "", "", "", "", "", ""));
            }

            // get ranked teams
            var teams = MaddenTable.FindTable(maddenDB.lTables, "TEAM").lRecords
                .Where(team => team["TBRK"].ToInt32() >= 1 && team["TBRK"].ToInt32() <= 126)
                .ToDictionary(team => team["TGID"].ToInt32(),
                team =>
                new
                {
                    Id = team["TGID"].ToInt32(),
                    Rank = team["TBRK"].ToInt32(),
                    ConfId = team["CGID"].ToInt32(),
                    Win = team["TSWI"].ToInt32(),
                    Loss = team["TSLO"].ToInt32(),
                });

            var champs = MaddenTable.FindTable(maddenDB.lTables, "CCHH").lRecords
                .OrderByDescending(c => c["SEYR"].ToInt32())
                .Take(10)
                .ToDictionary(c => c["TGID"].ToInt32(), c => c["CGID"].ToInt32());

            int i = 0;
            foreach (var kvp in teams.OrderBy(t => t.Value.Rank).Where(t => t.Value.Rank <= 25))
            {
                matchups[i] = string.Join(",", matchups[i], string.Empty, string.Empty, string.Empty, kvp.Value.Rank, RecruitingFixup.TeamNames[kvp.Value.Id]);

                if (champs.ContainsKey(kvp.Value.Id))
                {
                    matchups[i] += "," + RecruitingFixup.ConferenceNames[champs[kvp.Value.Id]];
                }

                i++;
            }

            i++;

            // get all the bowl teams
            var teamIds = new HashSet<int>(schedules.SelectMany(g => new[] { g.Value.Home, g.Value.Away }));
            /*var bowlTeams = teams.Values.Where(t => teamIds.Contains(t.Id) && t.Win <= 6).OrderBy(t => t.Win).ThenByDescending(t => t.Loss).ToArray();
            foreach (var t in bowlTeams)
            {
                matchups[i] = string.Join(",", matchups[i], string.Empty, string.Empty, string.Empty, string.Empty, RecruitingFixup.TeamNames[t.Id], string.Format("{0} -- {1}", t.Win, t.Loss));
                i++;
            }*/

            var bowlTeams = teams.Values.Where(t => teamIds.Contains(t.Id) && t.Rank > 25).OrderBy(t => t.Rank).ToArray();

            var sb = new StringBuilder();
            foreach (var team in bowlTeams)
            {
                matchups.Add(string.Join(",", RecruitingFixup.TeamNames[team.Id], RecruitingFixup.ConferenceNames[team.ConfId], string.Format("{0} -- {1}", team.Win, team.Loss)));
            }

            matchups.Add("************");

            var bowlEligibleTeams = teams.Values.Where(t => !teamIds.Contains(t.Id) && t.Rank > 25 && t.Win >= 5).OrderByDescending(t => t.Win).ThenBy(t => t.Loss).ThenBy(t => t.Rank).ToArray();
            foreach (var team in bowlEligibleTeams)
            {
                matchups.Add(string.Join(",", RecruitingFixup.TeamNames[team.Id], RecruitingFixup.ConferenceNames[team.ConfId], string.Format("{0} -- {1}", team.Win, team.Loss)));
            }

            File.WriteAllLines("bowlmatchups.csv", matchups);
        }

        private void bowlMatchupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var games = MaddenTable.FindTable(maddenDB.lTables, "BOWL").lRecords.Where(r => r["SEWN"].ToInt32() > 16).OrderBy(b => b["BNME"]).ToArray();
            var sb = new StringBuilder();
            foreach (var bowl in games)
            {
                var s = string.Join(",", bowl["BNME"], bowl["BCI1"].ToConferenceName() + " #" + bowl["BCR1"], bowl["BCI2"].ToConferenceName() + " #" + bowl["BCR2"]);
                sb.AppendLine(s);
            }

            File.WriteAllText("bowlgames.csv", sb.ToString());
        }

        private void dumpNameFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var players = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords;
            var fn = new Dictionary<int, HashSet<string>>();
            fn[0] = new HashSet<string>(StringComparer.Ordinal);
            fn[1] = new HashSet<string>(StringComparer.Ordinal);
            fn[2] = new HashSet<string>(StringComparer.Ordinal);

            var ln = new Dictionary<int, HashSet<string>>();
            ln[0] = new HashSet<string>(StringComparer.Ordinal);
            ln[1] = new HashSet<string>(StringComparer.Ordinal);
            ln[2] = new HashSet<string>(StringComparer.Ordinal);

            foreach (var player in players)
            {
                var key = FaceToKey(player["PGHE"].ToInt32());
                var first = player["PFNA"].Replace("'", "");
                var last = player["PLNA"].Replace("'", "").Replace("Jr.", "").Replace(" II", "").Replace(" III", "").Replace(" IV", "").Replace(" V", "");

                if (first.Length <= 9)
                    fn[key].Add(first);

                if (last.Length <= 12)
                    ln[key].Add(last);
            }

            var names = new Dictionary<string, string[]>()
            {
                { "LDFN", fn[0].ToArray() },
                { "MDFN",  fn[1].ToArray()},
                { "DDFN", fn[2].ToArray()},
                { "LDLN", ln[0].ToArray()},
                { "MDLN", ln[1].ToArray()},
                { "DDLN", ln[2].ToArray()},
            };

            names.ToJsonFile("rosternames.txt");
        }

        static int FaceToKey(int face)
        {
            if (face <= 99) return 0;
            if (face <= 159) return 1;
            return 2;
        }

        private void addFCSGamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeamEntry entry = new TeamEntry();
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK && entry.TeamId > 0)
            {
                var schd = MaddenTable.FindTable(maddenDB.lTables, "SCHD");
                var currentSeason = schd.lRecords.Where(r => r["SEYR"].ToInt32() == 0).First()["SESI"];

                for (int i = 0; i < entry.TeamId; i++)
                {
                    var week = i % 13;

                    //if (i > 4)
                    //  week = 13;

                    var a = (160 + (i % 5));
                    var b = (160 + (i % 5));
                    var mr = schd.AddNewRecord();
                    mr["GSTA"] = "1";
                    mr["GASC"] = "0";
                    mr["GHSC"] = "0";
                    mr["SGID"] = "0";
                    mr["GTOD"] = "720";
                    mr["GUTE"] = "0";
                    mr["GATG"] = a.ToString();
                    mr["GHTG"] = b.ToString();
                    mr["SESI"] = currentSeason; // schd.lRecords[0]["SESI"];
                    mr["CPCK"] = "7";
                    mr["HPCK"] = "7";
                    mr["SGNM"] = i.ToString();
                    mr["SEWN"] = week.ToString();
                    mr["SEWT"] = week.ToString();
                    mr["SEYR"] = "0";
                    mr["GDAT"] = "5";
                    mr["GFOT"] = "0";
                    mr["GFFU"] = "0";
                    mr["GFHU"] = "0";
                    mr["GMFX"] = "1";
                }
            }
            else
            {

            }
        }

        private void copyRecruitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeamEntry entry = new TeamEntry();
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // for a given recruit, read all his data
                var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");
                var recruit = recruitTable.lRecords.Where(r => r["RCRK"].ToInt32() == entry.TeamId).Single();

                if (recruit["PYEA"].ToInt32() == 3)
                {
                    MessageBox.Show("Eligbility exhausted");
                    return;
                }

                var ignoreFields = new HashSet<string>(new[] {/*"RATH", "RPGP",*/ "PRSI", "RCRK", "RCPR", "RCCB" });
                var dict = new Dictionary<string, string>();

                foreach (var f in recruitTable.lFields.Where(f => !ignoreFields.Contains(f.Abbreviation)))
                {
                    dict[f.Abbreviation] = recruit[f.Abbreviation];
                    dict["RCRK"] = "500";
                }

                dict.ToJsonFile("R" + entry.TeamId + ".txt");
            }
        }


        private static string[] PlayerSkills = new[] { "PSPD", "PSTR", "PAGI", "PACC", "PAWR", "PBTK", "PTRK", "PESV", "PBCV", "PSAR", "PSMV", "PJMV", "PCAR", "PCTH", "SPCT", "TRAF", "PRTR", "PJMP", "PTHP", "PTHA", "PTAK", "PHIT", "PPMV", "PFMV", "PBSH", "PPRS", "PPRC", "PMCV", "PZCV", "PYRS", "RELS", "PPBK", "PRBK", "PIBL", "PKRT", "PSTA", "PINJ" };

        private void randomizeNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var first = new HashSet<string>();
            var last = new HashSet<string>();

            // rtg names begin at 42700
            var players = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "PLAY");
            foreach (var p in players.lRecords)
            {
                first.Add(p["PFNA"]);
                last.Add(p["PLNA"]);
            }

            var fn = first.ToArray();
            var ln = last.ToArray();

            for (int i = 42700; i <= 42768; i++)
            {
                var player = players.lRecords.Where(r => r["PGID"].ToInt32() == i).Single();

                var fidx = RAND(fn.Length);
                var lidx = RAND(ln.Length);

                player["PFNA"] = fn[fidx];
                player["PLNA"] = ln[lidx];
            }
        }

        private void applyRosterStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var set = new HashSet<string>(PlayerSkills);


            TeamEntry entry = new TeamEntry();

            // pick the recruit id
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // pick the file
                FileDialog fd = new OpenFileDialog();

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    var dict = fd.FileName.FromJsonFile<Dictionary<string, string>>();

                    // find the playaer
                    var players = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "PLAY");
                    var fields = new HashSet<string>(players.lFields.Select(f => f.Abbreviation), StringComparer.OrdinalIgnoreCase);
                    var player = players.lRecords.Where(r => r["PGID"].ToInt32() == entry.TeamId).Single();

                    foreach (var kvp in dict)
                    {
                        if (!fields.Contains(kvp.Key))
                            continue;

                        player[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        private void applyRecruitFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var set = new HashSet<string>(PlayerSkills);

            TeamEntry entry = new TeamEntry();

            // pick the recruit id
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // pick the file
                FileDialog fd = new OpenFileDialog();

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    var dict = fd.FileName.FromJsonFile<Dictionary<string, string>>();

                    // find the recruit
                    var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");
                    var recruit = recruitTable.lRecords.Where(r => r["PRSI"].ToInt32() == entry.TeamId).Single();
                    recruit["RCRK"] = (Rand100() + 500).ToString();

                    // add a modifier to unscouted OVR of 0 to 5 because JUCOs have some game tape
                    dict["RCOV"] = (dict["POVR"].ToInt32() + (Rand100() % 6)).ToString();

                    foreach (var kvp in dict)
                    {
                        // increment the player year
                        if (kvp.Key == "PYEA")
                        {
                            recruit[kvp.Key] = (kvp.Value.ToInt32() + 1).ToString();
                        }
                        else if (set.Contains(kvp.Key))
                        {
                            // add 0-2 points for offseason progression
                            var mod = Rand100() % 3;
                            var newValue = Math.Min(99, kvp.Value.ToInt32() + mod);
                            recruit[kvp.Key] = newValue.ToString();
                        }
                        else
                        {
                            recruit[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
        }

        private void redshritFroshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // find all the freshman players
            var playerTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "PLAY");

#if false
            // any players without a redshirt are set to current season, allowing for 5 years of eligibility
            var playersToRedshirt = playerTable.lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["PRSD"].ToInt32() == 0).ToList();

            foreach (var player in playersToRedshirt)
            {
                player["PRSD"] = "1";
            }
#else


            // find true frosh without redshirt on
            var trueFrosh = playerTable.lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["PYEA"].ToInt32() == 0 && mr["PRSD"].ToInt32() == 0).ToList();

            var froshDict = trueFrosh.ToDictionary(f => f["PGID"].ToInt32(), f => 0);
            var playerDict = trueFrosh.ToDictionary(f => f["PGID"].ToInt32());

            FindGamesPlayed(froshDict, "PSDE");
            FindGamesPlayed(froshDict, "PSOF");
            FindGamesPlayed(froshDict, "PSOL");
            FindGamesPlayed(froshDict, "PSKI");
            FindGamesPlayed(froshDict, "PSKP");

            var playersToRedshirt = froshDict.Where(kvp => kvp.Value <= 4).ToArray();
            foreach (var player in playersToRedshirt)
            {
                playerDict[player.Key]["PRSD"] = "1";
            }
#endif
        }

        static int? CurrentYear { get; set; } = null;

        public static int FindCurrentYear()
        {
            if (!CurrentYear.HasValue)
            {
                var table = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "CCHH");

                int yr = 0;

                foreach (var mr in table.lRecords)
                {
                    yr = Math.Max(yr, mr["SEYR"].ToInt32());
                }

                CurrentYear = yr;
            }

            return CurrentYear.Value;
        }

        private void FindGamesPlayed(Dictionary<int, int> dict, string table)
        {
            var year = FindCurrentYear();
            var statTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, table);

            foreach (var mr in statTable.lRecords)
            {
                // SEYR needs to be the current year - 1 for this to count
                if (mr["SEYR"].ToInt32() != year) continue;

                var playerId = mr["PGID"].ToInt32();
                var gamesPlayed = mr["sgmp"].ToInt32();

                // player is in the dict and the value in the dict is less than the games played
                if (dict.TryGetValue(playerId, out var currValue) && currValue < gamesPlayed)
                {
                    dict[playerId] = gamesPlayed;
                }
            }
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var dict = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var table in maddenDB.lTables)
            {
                var name = table.Abbreviation ?? table.Name;

                var tbl = new List<Dictionary<string, string>>();
                dict[name] = tbl;

                var columns = table.lFields.Select(f => f.Abbreviation).ToArray();

                foreach (var mr in table.lRecords)
                {
                    var row = new Dictionary<string, string>();
                    foreach (var c in columns)
                    {
                        row[c] = mr[c];
                    }

                    tbl.Add(row);
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.Filter = "*.txt|*.*";
            saveFileDialog.Title = "Save playbook as...";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                dict.ToJsonFile(saveFileDialog.FileName);
            }
        }

        private void importToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "*.txt|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select a playbook to import...";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                // this appears to be a book id, which should not change
                const string BOKL = "BOKL";

                var dict = openFileDialog.FileName.FromJsonFile<Dictionary<string, List<Dictionary<string, string>>>>();

                var myBOKL = dict.First().Value.First()[BOKL];

                foreach (var table in maddenDB.lTables)
                {
                    // don't change the playbook id
                    var playbookId = table.lRecords[0][BOKL];
                    var name = table.Abbreviation ?? table.Name;

                    // get the dict
                    var tbl = dict[name];

                    table.Clear();

                    foreach (var row in tbl)
                    {
                        var mr = table.AddNewRecord();

                        foreach (var kvp in row)
                        {
                            mr[kvp.Key] = kvp.Value;
                        }

                        if (mr[BOKL] == myBOKL)
                        {
                            mr[BOKL] = playbookId;
                        }
                    }
                }
            }
        }

        // 42 is the sun belt ccg
        // 41 is the AAC ccg
        const int ElevenTeamCCGId = 41;

        // 3 is the AAC id
        // 13 is the Sun Belt id
        const int ConferenceThatHas11Teams = 3;

        // 900 is start time for AAC
        // 780 is start time for SBC
        const string StartTime = "900";

        // friday is 4, sat is 5
        const string GameDay = "4";

        private void setSunBeltCCGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("FCYR should be the same as start of season", "decrement conference championships!!");
            TeamEntry homeEntry = new TeamEntry("Home team");
            TeamEntry awayEntry = new TeamEntry("Away team");
            if (homeEntry.ShowDialog() == DialogResult.OK)
            {
                if (awayEntry.ShowDialog() == DialogResult.OK)
                {
                    //find stadium id for home team
                    var home = homeEntry.TeamId;
                    var away = awayEntry.TeamId;
                    var gameNum = "43";
                    var week = "16";
                    var teamQuery = new Dictionary<string, string>();
                    teamQuery["TGID"] = home.ToString();
                    var teamRecord = MaddenTable.Query(Form1.MainForm.maddenDB.lTables, "TEAM", teamQuery).SingleOrDefault();
                    var teamStadium = teamRecord["SGID"];

                    // create a game 
                    var schd = MaddenTable.FindTable(maddenDB.lTables, "SCHD");
                    var currentSeason = schd.lRecords.Where(r => r["SEYR"].ToInt32() == 0).First()["SESI"];

                    // create the record
                    var mr = schd.AddNewRecord();
                    mr["GSTA"] = "1";
                    mr["GASC"] = "0";
                    mr["GHSC"] = "0";
                    mr["SGID"] = teamStadium.ToString();
                    mr["GTOD"] = StartTime;
                    mr["GUTE"] = "0";
                    mr["GATG"] = away.ToString();
                    mr["GHTG"] = home.ToString();
                    mr["SESI"] = "0";
                    mr["CPCK"] = "7";
                    mr["HPCK"] = "7";
                    mr["SGNM"] = gameNum;
                    mr["SEWN"] = week.ToString();
                    mr["SEWT"] = week.ToString();
                    mr["SEYR"] = "0";
                    mr["GDAT"] = GameDay;
                    mr["GFOT"] = "0";
                    mr["GFFU"] = "0";
                    mr["GFHU"] = "0";
                    mr["GMFX"] = "0";
                    mr["SGID"] = teamStadium;

                    var query = new Dictionary<string, string>();
                    query["SGNM"] = gameNum;
                    query["SEWN"] = week;

                    // set the team schedules
                    var teamScheduleTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TSCH");
                    query = new Dictionary<string, string>();
                    query["TGID"] = home.ToString();
                    query["SEWN"] = week;

                    var homeTeamSchedule = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
                    homeTeamSchedule["OGID"] = away.ToString();
                    homeTeamSchedule["THOA"] = "1";
                    homeTeamSchedule["SGNM"] = gameNum;

                    query["TGID"] = away.ToString();
                    var awayTeamSchedule = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
                    awayTeamSchedule["OGID"] = home.ToString();
                    awayTeamSchedule["THOA"] = "1";
                    awayTeamSchedule["SGNM"] = gameNum;

                    // modify the bowl game table
                    var bowlGameTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "BOWL");
                    query = new Dictionary<string, string>();
                    query["BIDX"] = ElevenTeamCCGId.ToString();

                    var ccg = MaddenTable.Query(bowlGameTable, query).SingleOrDefault();
                    ccg["SGNM"] = gameNum;
                    ccg["SGID"] = teamStadium;
                }
            }
        }

        private void transferRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var transferTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TRAN");

            foreach (var mr in transferTable.lRecords)
            {
                mr["TRYR"] = "1";
            }
        }

        private void dumpRostersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DumpRosters();
        }

        private void cleanupCCHHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // find the CCHH table
            var table = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "CCHH");

            TeamEntry champ = new TeamEntry("Conference Champ");
            if (champ.ShowDialog() == DialogResult.OK)
            {
                var records = table.lRecords.OrderByDescending(mr => mr["SEYR"].ToInt32()).Where(mr => mr["CGID"].ToInt32() == ConferenceThatHas11Teams).Take(2).ToArray();

                // find the correct one
                bool found = false;

                foreach (var record in records)
                {
                    // once we find it, set the flag and continue
                    if (record["CGID"].ToInt32() == ConferenceThatHas11Teams && record["TGID"].ToInt32() == champ.TeamId && !found)
                    {
                        found = true;
                        continue;
                    }

                    // remove the record
                    table.RemoveRecord(record);
                }
            }
        }

        private void customFixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string stadiumFile = "jmu-stadium.txt";
            const string teamFile = "jmu-team.txt";

            // fiu stadium is 241
            var stadiumTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "STAD");
            var teamTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "TEAM");
            var jmuTeam = teamTable.lRecords.Where(r => r["TGID"].ToInt32() == 230).Single();
            var jmuStadium = stadiumTable.lRecords.Where(r => r["SGID"].ToInt32() == 241).Single();
#if false // copy JMU from v21
            var dict = new Dictionary<string, string>();
            var include = new HashSet<string>(RosterCopy.STADIUM_DATA_TO_COPY, StringComparer.OrdinalIgnoreCase);

            foreach (var f in stadiumTable.lFields)
            {
                if (include.Contains(f.Abbreviation))
                {
                    dict[f.Abbreviation] = jmuStadium[f.Abbreviation];
                }
            }

            dict.ToJsonFile(stadiumFile);

            dict = new Dictionary<string, string>();

            foreach (var f in teamTable.lFields)
            {
                dict[f.Abbreviation] = jmuTeam[f.Abbreviation];
            }

            dict.ToJsonFile(teamFile);
#else // apply jmu changes
            var team = teamFile.FromJsonFile<Dictionary<string, string>>();
            var stad = stadiumFile.FromJsonFile<Dictionary<string, string>>();

            foreach (var kvp in team)
            {
                jmuTeam[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in stad)
            {
                jmuStadium[kvp.Key] = kvp.Value;
            }
#endif
        }


        private void cureBowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBowlGame(AdditionalGameProvider.CureBowl, 162);
        }

        private void myrtleBeachBowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBowlGame(AdditionalGameProvider.MyrtleBeachBowl, 60);
        }

        private void arizonaBowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBowlGame(AdditionalGameProvider.ArizonaBowl, 3);
        }

        private void venturesBowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBowlGame(AdditionalGameProvider.Sixty8VenturesBowl, 225);
        }

        private void AddBowlGame(int gameNumber, int stadium) => AddBowlGame(gameNumber, stadium);

        private void AddPlayoffGame(int gameNumber, string startTime, string day = "5") => AddBowlGame(gameNumber, 0, day, true, startTime);

        private void AddBowlGame(int gameNumber, int stadium, string day = "5", bool isPlayoffGame =false, string startTime = StartTime)
        {
            TeamEntry homeEntry = new TeamEntry("Home team");
            TeamEntry awayEntry = new TeamEntry("Away team");
            if (homeEntry.ShowDialog() == DialogResult.OK)
            {
                if (awayEntry.ShowDialog() == DialogResult.OK)
                {
                    //find stadium id for home team
                    var home = homeEntry.TeamId;
                    var away = awayEntry.TeamId;
                    var gameNum = gameNumber.ToString();
                    var week = "18";
                    var teamQuery = new Dictionary<string, string>();
                    teamQuery["TGID"] = home.ToString();
                    var teamRecord = MaddenTable.Query(Form1.MainForm.maddenDB.lTables, "TEAM", teamQuery).SingleOrDefault();
                    var teamStadium = isPlayoffGame ? teamRecord["SGID"] : stadium.ToString();

                    // create a game 
                    var schd = MaddenTable.FindTable(maddenDB.lTables, "SCHD");
                    var currentSeason = schd.lRecords.Where(r => r["SEYR"].ToInt32() == 0).First()["SESI"];

                    // create the record
                    var mr = schd.AddNewRecord();
                    mr["GSTA"] = "1";
                    mr["GASC"] = "0";
                    mr["GHSC"] = "0";
                    mr["SGID"] = teamStadium.ToString();
                    mr["GTOD"] = startTime;
                    mr["GUTE"] = "0";
                    mr["GATG"] = away.ToString();
                    mr["GHTG"] = home.ToString();
                    mr["SESI"] = "0";
                    mr["CPCK"] = "7";
                    mr["HPCK"] = "7";
                    mr["SGNM"] = gameNum;
                    mr["SEWN"] = week.ToString();
                    mr["SEWT"] = "30";
                    mr["SEYR"] = "0";
                    mr["GDAT"] = day;
                    mr["GFOT"] = "0";
                    mr["GFFU"] = "0";
                    mr["GFHU"] = "0";
                    mr["GMFX"] = "0";
                    mr["SGID"] = teamStadium;

                    // set the team schedules
                    var teamScheduleTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TSCH");
                    var query = new Dictionary<string, string>();
                    query["TGID"] = home.ToString();

                    var homeTeamSchedule = MaddenTable.Query(teamScheduleTable, query).Where(ts => ts["SEWN"].ToInt32() > 16).SingleOrDefault();

                    if (homeTeamSchedule == null)
                    {
                        homeTeamSchedule = teamScheduleTable.AddNewRecord();
                    }

                    homeTeamSchedule["OGID"] = away.ToString();
                    homeTeamSchedule["THOA"] = "1";
                    homeTeamSchedule["SGNM"] = gameNum;
                    homeTeamSchedule["SEWN"] = week;

                    query["TGID"] = away.ToString();
                    var awayTeamSchedule = MaddenTable.Query(teamScheduleTable, query).Where(ts => ts["SEWN"].ToInt32() > 16).SingleOrDefault();

                    if (awayTeamSchedule == null)
                    {
                        awayTeamSchedule = teamScheduleTable.AddNewRecord();
                    }

                    awayTeamSchedule["OGID"] = home.ToString();
                    awayTeamSchedule["THOA"] = isPlayoffGame ? "0" : "1";
                    awayTeamSchedule["SGNM"] = gameNum;
                    awayTeamSchedule["SEWN"] = week;
                }
            }
        }

        private void cleanupBCHHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // find the CCHH table
            var table = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "BCHH");
            var records = table.lRecords.Where(mr => mr["BIDX"].ToInt32() >= 45);

            foreach (var record in records)
            {
                table.RemoveRecord(record);
            }
        }

        private void add5V12ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddPlayoffGame(AdditionalGameProvider.CFP5v12, "1200");
        }

        private void add6V11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddPlayoffGame(AdditionalGameProvider.CFP6v11, "960");
        }

        private void add7V10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddPlayoffGame(AdditionalGameProvider.CFP7v10, "720");
        }

        private void add8V9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddPlayoffGame(AdditionalGameProvider.CFP8v9, "1200", "4");
        }
    }

    [DataContract]
    public class TeamRecord
    {
        /*
         * RBKS table
            RCDM - record team
            RCDI - record key e.g. INTs/REC/ETC
            RCDT - Record Type .  0=game, 1 = season , 2 = career
            RCDH = Record Holder
            RCDV = Record Value
            RCDO = Record Opponent
            RCDY = Record Year
         *  RCDE = Previous Holder
         * */
        [DataMember]
        public int TeamId { get; set; }

        //INTs/REC/ETC        
        [DataMember]
        public int Key { get; set; }

        //0=game, 1 = season , 2 = career       
        [DataMember]
        public int Type { get; set; }

        /// <summary>
        /// 2068 M.McDaniel
        /// </summary>
        [DataMember]
        public string Holder { get; set; }

        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public string Opponent { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public int DynastyYear { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RealYear { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PreviousRecordHolder { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Week { get; set; }

        public string RecordHolderValue
        {
            get
            {
                string lastName = Holder.Substring(Holder.IndexOf(' ') + 1);
                return string.Format("{0} {1}.{2}", RealYear, Holder[0], lastName);
            }
        }

        static char[] delimiter = new char[] { ' ', '.' };

        public override int GetHashCode()
        {
            return CreateKey(this.TeamId, this.Key, this.Type);
        }

        public override bool Equals(object obj)
        {
            var other = obj as TeamRecord;
            return other != null &&
                other.TeamId == this.TeamId &&
                other.Key == this.Key &&
                other.Type == this.Type;
        }

        public static int CreateKey(int id, int key, int type)
        {
            return type << 24 | key << 16 | id;
        }
    }


    [DataContract]
    public class HomeRecord
    {
        [DataMember]
        public int TeamId { get; set; }
        [DataMember]
        public int Win { get; set; }
        [DataMember]
        public int Loss { get; set; }
        [DataMember]
        public int Tie { get; set; }
    }


	public class BitStream
	{	public	byte[]		buffer;
		public	BitArray	bitarray;
		public	long		bitsused;


		public void AllocBits( long bitcount )
		{	buffer		= new byte [ (bitcount /8) ];
			bitsused	= bitcount;
			bitarray	= new BitArray( buffer );
		}
		public BitStream( )
		{	buffer		= null;
			bitsused	= 0;
			bitarray	= null;
		}
		public BitStream( long bitcount )
		{	AllocBits( bitcount );
		}
		public void Copy( BitStream bits )
		{	buffer		= new byte[ bits.buffer.Length ];
			bits.buffer.CopyTo( buffer, 0 );
			bitarray	= new BitArray( buffer );
		}

		public ulong ReadBits( long offset, long bitstoread )
		{	ulong	ret		= 0;
			ulong	mask	= 1;

			for( int i=(int)bitstoread; i>0; i-- )
			{	if( bitarray.Get( i +(int)offset -1 ) == true )
				{	ret	= ret | mask;
				}
				mask	= mask << 1;
			}
			return ret;
		}
		public void WriteBits( ulong value, long offset, long bitstowrite )
		{	ulong	mask	= 1;

			for( int i=(int)bitstowrite; i>0; i-- )
			{
				if( (mask & value) == mask )
					bitarray.Set( i +(int)offset -1, true );
				else
					bitarray.Set( i +(int)offset -1, false );
				mask	= mask << 1;
			}
		}
		private byte ReverseByte( byte b )
		{	byte	r		= 0;
			for( int i=0; i<8; i++ )
			{	r	= (byte)(r << 1);
				if( (b & 0x01) == 1 )
				{	r	|= 0x01;
				}
				b	= (byte)(b >> 1);
			}
			return r;
		}
		private byte[] StringToBuffer( string s, int maxchars )
		{	byte[]	buffer	= new byte[ maxchars ];
			char[]	str		= s.ToCharArray( );

			for( int i=0; i<maxchars && i<s.Length; i++ )
			{	buffer[i]	= (byte) str[i];
			}
			return buffer;
		}
		public void StoreBytes( string bytes, long offset, int bytestowrite )
		{	byte[]	buffer	= StringToBuffer( bytes, bytestowrite );
			for( int i=0; i<bytestowrite; i++ )
				WriteBits( ( (ulong)(buffer[i]) & 0x000000FF ), offset +(i *8), 8 );
		}
		public void ActivateBits( )
		{	// reverse each byte for this
			byte[]	nbuf	= new byte[ buffer.Length ];
			for( int i=0; i<buffer.Length; i++ )
			{	nbuf[i]	= ReverseByte( buffer[i] );
			}
			bitarray	= new BitArray( nbuf );
		}
		public void DeactivateBits( )
		{	byte[]	nbuf	= new byte[ buffer.Length ];
			for( int i=0; i<buffer.Length; i++ )
			{	nbuf[i]	= ( (byte)ReadBits( i *8, 8 ) );
			}
			buffer	= nbuf;
		}

		public string PrintBits( )
		{	string	ret		="";
			int		count1	= 0;

			foreach( Object obj in bitarray )
			{
				if( (count1 % 80) == 0 )
					ret	+= "\n" + (count1 / 80) + "\t";
				if( (count1 % 8) == 0 )
					ret += " ";
				if( obj.ToString( ) == "True" )
					ret += "1";
				else
					ret	+= "0";
				count1++;
			}

			ret	+= "\n\n\n";
			count1	= 0;

			foreach( Object obj in bitarray )
			{
				if( (count1 % 80) == 0 )
					ret	+= "\n" + (count1 / 80) + "\t";
				if( obj.ToString( ) == "True" )
					ret += "1";
				else
					ret	+= "0";
				count1++;
			}

			return ret;
		}
	}
	public class Field
	{
#region members
		public string	name;
		public ulong	type;
		public ulong	offset;
		public ulong	bits;

		// for experimental XML config mapping
		public string	Abbreviation	= "";
		public string	Name			= "";
		public string	Description		= "";
		public string	ControlType		= "";
		public Control	EditControl		= new TextBox( );
		public int		ControlItems	= 0;
		public string	ControlLink		= "";
		public string	ControlIF		= "";
		public string	ControlRF		= "";
		public string	ControlRF2		= "";
		public bool		ControlLocked	= false;
		public int		Offset			= 0;
#region new for calculated type, 10/23/13
		public class Variable
		{	public string	vField		= "";
			public double	Multiplier	= 0.0;

			public double Calc( List<Field> lMappedFields, MaddenRecord record )
			{	double	r	= 0.0;
				Field	f	= FindField( lMappedFields, vField );

				if( f != null )
				{	double	v	= Convert.ToDouble( record[ f.Abbreviation ] );
					r			= v * Multiplier;
				}

				return r;
			}
			public static List<Variable> ReadVariables( XmlTextReader reader, string Path )
			{
				List<Variable>	vars	= new List<Variable>( );
				Variable		var		= null;

				while( reader.Read( ) )
				{
					switch( reader.NodeType )
					{
						case XmlNodeType.Element:
							if( reader.Name == "Variable" )
								var	= new Variable( );

							Path	+= reader.Name + "\\";
							break;

						case XmlNodeType.Text:
							if( Path.EndsWith( "Variable\\Field\\" ) )
								var.vField		= reader.Value;

							if( Path.EndsWith( "Variable\\Multiplier\\" ) )
								var.Multiplier	= Convert.ToDouble( reader.Value );
							break;

						case XmlNodeType.EndElement:
							if( reader.Name == "Variable" )
								vars.Add( var );

							try
							{	Path	= Path.Remove( Path.LastIndexOf( reader.Name + "\\" ) );
							}	catch( Exception e )
							{
								MessageBox.Show( "XML closing element not found: " + reader.Name + ", " + reader.LineNumber, "Error in XML config" );
								throw( e );
							}
							if( reader.Name == "Variables" )	// should only ever return from here
								return vars;
						
							break;
					}

				}
				// bad XML!
				throw( new Exception( "Closing Variables Tag Missing! Exiting!" ) );
			}
		}
		public class Formula
		{
			public string			IndexValue	= "";
			public List<Variable>	Variables	= new List<Variable>( );
			public double			Adjustment	= 0.0;

			public double Calc( List<Field> lMappedFields, MaddenRecord record )
			{	double	r	= 0.0;

				foreach( Variable v in Variables )
					r	+= v.Calc( lMappedFields, record );

				return r + Adjustment;
			}
			public static List<Formula> ReadFormulas( XmlTextReader reader, string Path )
			{
				List<Formula>	forms	= new List<Formula>( );
				Formula			form	= null;

				while( reader.Read( ) )
				{
					switch( reader.NodeType )
					{
						case XmlNodeType.Element:
							if( reader.Name == "Formula" )
								form	= new Formula( );

							if( reader.Name == "Variables" )
							{	form.Variables	= Variable.ReadVariables( reader, Path + "Variables\\" );
								break;
							}

							Path	+= reader.Name + "\\";
							break;

						case XmlNodeType.Text:
							if( Path.EndsWith( "Formula\\IndexValue\\" ) )
								form.IndexValue	= reader.Value;

							if( Path.EndsWith( "Formula\\Adjustment\\" ) )
								form.Adjustment	= Convert.ToDouble( reader.Value );
							break;

						case XmlNodeType.EndElement:
							if( reader.Name == "Formula" )
								forms.Add( form );

							try
							{	Path	= Path.Remove( Path.LastIndexOf( reader.Name + "\\" ) );
							}	catch( Exception e )
							{
								MessageBox.Show( "XML closing element not found: " + reader.Name + ", " + reader.LineNumber, "Error in XML config" );
								throw( e );
							}
							if( reader.Name == "Formulas" )	// should only ever return from here
								return forms;
						
							break;
					}

				}
				// bad XML!
				throw( new Exception( "Closing Formulas Tag Missing! Exiting!" ) );
			}
		}
		public double			Min			= 0.0;
		public double			Max			= 0.0;
		public	List<Formula>	Formulas	= new List<Formula>( );

		// only if this is a Calculated type should this function be called
        public double RunFormula(List<Field> lMappedFields, MaddenRecord record)
        {
            double r = 0.0;
            Field f = FindField(lMappedFields, ControlIF);

            if (f == null)
                return r;

            Formula form = Formulas.Find((a) => a.IndexValue == record[f.Abbreviation]);
            if (form == null)
                form = Formulas.Find((a) => a.IndexValue == "*");
            if (form != null)
                r = form.Calc(lMappedFields, record);
            r = Math.Min(r, Max);
            r = Math.Max(r, Min);
            r = Math.Round(r, 0);

            if (RecruitingFixup.PreseasonFixupRun && this.Name == "OVR*")
            {
                if (Math.Abs(((int)r) - record["POVR"].ToInt32()) > 12)
                {
                    var variance = RecruitingFixup.RAND.Next(-5, 6);
                    var preScout = (int)r - variance;

                    if (preScout > 99) preScout = 99; 

                    record["RCOV"] = preScout.ToString();
                    record["POVR"] = r.ToString();
                }
            }

            return r;
        }
#endregion
		// new attempt to speed up searches with ref objs
		public Dictionary<string,int>		KeyToIndexMappings	= new Dictionary<string,int>( );
#endregion

		public static int	fieldsize	= 16;

		public enum FieldType
		{
			tdbString = 0,
			tdbBinary = 1,
			tdbSInt = 2,
			tdbUInt = 3,
			tdbFloat = 4,
		}

		public Field( )
		{	name		= "";
			type		= 0;
			offset		= 0;
			bits		= 0;
		}
		public Field( string Name, FieldType Type, ulong Offset, ulong Bits )
		{	name		= Name;
			type		= (ulong) Type;
			offset		= Offset;
			bits		= Bits;
		}
		public override string ToString()
		{
			if( Name != null && Name != "" )					return Name;
			if( Abbreviation != null && Abbreviation != "" )	return Abbreviation;
			if( name != null )
				return name;

			return base.ToString();
		}
		public int CompareDataAsType( string data1, string data2 )
		{	int	returnVal	= 0;

			//if( this.ControlType == "ComboBox" && data1.IndexOf( '(' ) >-1 && data1.IndexOf( ')' ) >-1 )
			//{	data1	= data1.Substring( data1.IndexOf( '(' ) +1 );
			//    data1	= data1.Substring( 0, data1.IndexOf( ')' ) );
			//    data2	= data2.Substring( data2.IndexOf( '(' ) +1 );
			//    data2	= data2.Substring( 0, data2.IndexOf( ')' ) );
			//}

			switch( type )
			{
				case (ulong) Field.FieldType.tdbString:
				case (ulong) Field.FieldType.tdbBinary:
					returnVal	= String.Compare( data1, data2 );
					break;

				case (ulong) Field.FieldType.tdbSInt:
				case (ulong) Field.FieldType.tdbUInt:
					returnVal	= Convert.ToInt32( data1 ).CompareTo( Convert.ToInt32( data2 ) );
					break;

				case (ulong) Field.FieldType.tdbFloat:
					returnVal	= Convert.ToDouble( data1 ).CompareTo( Convert.ToDouble( data2 ) );
					break;
			}
			return returnVal;
		}

		public void WriteField( byte[] buf, int _offset )
		{	char[]	arr	= name.ToCharArray( );

			DBFileInfo.WriteDW2Buf( buf, _offset +0, (uint) type );
			DBFileInfo.WriteDW2Buf( buf, _offset +4, (uint) offset );

			buf[ _offset + 8 ]	= (byte) arr[3];
			buf[ _offset + 9 ]	= (byte) arr[2];
			buf[ _offset +10 ]	= (byte) arr[1];
			buf[ _offset +11 ]	= (byte) arr[0];

			DBFileInfo.WriteDW2Buf( buf, _offset +12, (uint) bits );
		}

        public static Field ReadEntry(FileStream fs, int offset)
        {
            byte[] array = new byte[fieldsize];
			Field	field	= new Field( );

			fs.Position		= offset;
			fs.Read( array, 0, fieldsize );
			field.type		= (((ulong)array[3]) ) | (((ulong)array[2]) << 8 ) | (((ulong)array[1]) << 16 ) | (((ulong)array[0]) << 24 );
			field.offset	= (((ulong)array[7]) ) | (((ulong)array[6]) << 8 ) | (((ulong)array[5]) << 16 ) | (((ulong)array[4]) << 24 );
			field.bits		= (((ulong)array[15]) ) | (((ulong)array[14]) << 8 ) | (((ulong)array[13]) << 16 ) | (((ulong)array[12]) << 24 );
			field.name		= Convert.ToChar( array[11] ).ToString( ) + Convert.ToChar( array[10] ).ToString( ) + Convert.ToChar( array[9] ).ToString( ) + Convert.ToChar( array[8] ).ToString( );
			return field;
		}
        public static List<Field> ReadFields(DBTable table, FileStream fs)
        {
            List<Field> lFields = new List<Field>();

			for( int i=0; i< table.numfields; i++ )
			{	Field	f	= ReadEntry( fs, (int) table.fieldStart + ( i * fieldsize ) );
				lFields.Add( f );
			}

			return lFields;
		}
		public static Field GetField( List<Field> lFields, string name )
		{	foreach( Field f in lFields )
			{	if( f.name == name )
					return f;
			}
			return null;
		}
		public static byte[] ReadBytes( Field f, BitStream bits )
		{	byte[]	buf	= new byte[ f.bits/8 ];

			for( int i=0; i < (int)(f.bits/8); i++ )
			{	buf[i]	= bits.buffer[ (int)(f.offset / 8) +i ];
			}
			return buf;
		}
		public static string ReadString( Field f, BitStream bits )
		{	string	buf	= "";

			for( int i=0; i < (int)(f.bits/8); i++ )
			{	byte	b	= bits.buffer[ (int)(f.offset / 8) +i ];
				if( b != 0 )
					buf	+= Convert.ToChar( b ).ToString( );
			}
			return buf;
		}
		public static byte[] ReadBytes( List<Field> lFields, string name, BitStream bits )
		{	Field	f	= Field.GetField( lFields, name );
			return ReadBytes( f, bits );
		}
		public static string ReadString( List<Field> lFields, string name, BitStream bits )
		{	Field	f	= Field.GetField( lFields, name );
			return ReadString( f, bits );
		}

		public static Field GetFieldByName( List<Field> lFields, string name )
		{	foreach( Field f in lFields )
			{	if( f.Name == name )
					return f;
			}
			return null;
		}
		public static Field GetFieldByAbbreviation( List<Field> lFields, string name )
		{	foreach( Field f in lFields )
			{	if( f.Abbreviation == name || f.name == name )
					return f;
			}
			return null;
		}
		public static Field FindField( List<Field> lFields, string Name )
		{
			Field	f	= GetFieldByName( lFields, Name );
			if( f != null )
				return f;
			return GetFieldByAbbreviation( lFields, Name );
		}

	}
	public class DBData
	{	public	Field		field		= null;
		private	string		_str		= "";
		private ulong		_ulong		= 0;
		private byte[]		array		= null;

		
		public DBData( ){}
		public DBData( Field f )
		{	SetField( f );
		}
		public DBData( Field f, BitStream bits )
		{	SetField( f );
			ReadData( bits );
		}
		public DBData( string fieldname, int type, string data )
		{	field		= new Field( );
			field.name	= fieldname;
			field.type	= (ulong) type;
			if( type == (int) Field.FieldType.tdbString )
				_str	= data;
			else
				_ulong	= Convert.ToUInt32( data );
		}
		public void SetField( Field f ){	field	= f; }
		public string GetFieldName( ){		return field.name; }
		public void ReadData( BitStream bits )
		{
			switch( field.type )
			{
				case (ulong) Field.FieldType.tdbString:
					_str	= Field.ReadString( field, bits );
					break;
				case (ulong) Field.FieldType.tdbBinary:
					array	= Field.ReadBytes( field, bits );
					break;
				case (ulong) Field.FieldType.tdbSInt:
				case (ulong) Field.FieldType.tdbUInt:
				case (ulong) Field.FieldType.tdbFloat:
					_ulong	= bits.ReadBits( (long)field.offset, (long)field.bits );
					break;
            }
        }
		public void WriteData( BitStream bits )
		{
			switch( field.type )
			{
				case (ulong) Field.FieldType.tdbString:
					bits.StoreBytes( _str, (long) field.offset, (int) field.bits /8 );
					break;

				case (ulong) Field.FieldType.tdbBinary:	// need to do this one day I suppose
					break;
				case (ulong) Field.FieldType.tdbSInt:
				case (ulong) Field.FieldType.tdbUInt:
				case (ulong) Field.FieldType.tdbFloat:
					bits.WriteBits( _ulong, (long) field.offset, (int) field.bits );
					break;
			}
		}
		public string Data
		{	get
			{
				switch( field.type )
				{
//                    case (ulong)Field.FieldType.tdbVarChar:
                    case (ulong) Field.FieldType.tdbString:
						return _str;

					case (ulong) Field.FieldType.tdbBinary:	// major assumption made that all binary fields are %8 =0
						string	s	= "";
						foreach( byte b in array )
							s	+= b.ToString( "X2" );
						return s;

					case (ulong) Field.FieldType.tdbSInt:
						return ((int) _ulong).ToString( );

					case (ulong) Field.FieldType.tdbUInt:
						return ((uint) _ulong).ToString( );

					case (ulong) Field.FieldType.tdbFloat:
						return ((float) _ulong).ToString( "F" );
				}
				return "";
			}
			set
			{
				switch( field.type )
				{
  //                  case (ulong)Field.FieldType.tdbVarChar:
                    case (ulong) Field.FieldType.tdbString:
						_str	= value;
						break;

					case (ulong) Field.FieldType.tdbBinary:
						char[]	temp	= ((string)value).ToCharArray( );
						if( array == null )
							array		= new byte[ temp.Length /2 ];
						for( int i=0; i < (temp.Length /2); i++ )
						{	array[ i ]	= Convert.ToByte( temp[ (i *2) +0 ].ToString( ) + temp[ (i *2) +1 ].ToString( ), 16 );
						}
						break;

					case (ulong) Field.FieldType.tdbSInt:
						_ulong	= Convert.ToUInt32( value );
						break;

					case (ulong) Field.FieldType.tdbUInt:
						_ulong	= Convert.ToUInt32( value );
						break;

					case (ulong) Field.FieldType.tdbFloat:
						_ulong	= Convert.ToUInt32( value );
						break;
				}

			}
		}
		public override string ToString()
		{
			return field.name + ":" + Data;
		}
	}
	public class DBTable
	{
		public string	TableName;							// 1st 4 bytes
		public UInt32	offsetFromIndex;					// from end of table index

		public int		headersize		= 8;

		public UInt32	priorcrc;							// checksum?
		public UInt32	unknown_2		= 0x00000006;		// type?
		public UInt32	len_bytes;							// size of each record in bytes
		public UInt32	len_bits;							// size of each record in bits
		public UInt32	zero			= 0;
		public UInt16	maxrecords;
		public UInt16	currecords;
		public UInt32	unknown_3		= 0x0000ffff;
		public byte		numfields;
		public byte		indexcount		= 0;
		public UInt16	zero2			= 0;
		public UInt32	zero3			= 0;
		public UInt32	headercrc;							// or crc poly?

		public int		infosize		= 40;

		public long		fieldStart		= 0;
		public long		dataStart		= 0;

		public UInt32	calcPcrc		= 0;
		public UInt32	calcHcrc		= 0;


		public DBTable( )
		{	TableName		= "";
			offsetFromIndex	= 0;
			priorcrc		= 0;
			unknown_2		= 6;
			len_bytes		= 0;
			len_bits		= 0;
			zero			= 0;
			maxrecords		= 0;
			currecords		= 0;
			unknown_3		= 0x0000ffff;
			numfields		= 0;
			indexcount		= 0;
			zero2			= 0;
			zero3			= 0;
			headercrc		= 0;
		}
        public DBTable(FileStream fs)
		{	TableName		= "";
			offsetFromIndex	= 0;
			priorcrc		= 0;
			unknown_2		= 0;
			len_bytes		= 0;
			len_bits		= 0;
			zero			= 0;
			maxrecords		= 0;
			currecords		= 0;
			unknown_3		= 0;
			numfields		= 0;
			indexcount		= 0;
			zero2			= 0;
			zero3			= 0;
			headercrc		= 0;

			ReadTableDefinition( fs );
		}
		public DBTable( DBTable org )
		{	TableName		= org.TableName;
			offsetFromIndex	= org.offsetFromIndex;
			priorcrc		= org.priorcrc;
			unknown_2		= org.unknown_2;
			len_bytes		= org.len_bytes;
			len_bits		= org.len_bits;
			zero			= org.zero;
			maxrecords		= org.maxrecords;
			currecords		= org.currecords;
			unknown_3		= org.unknown_3;
			numfields		= org.numfields;
			indexcount		= org.indexcount;
			zero2			= org.zero2;
			zero3			= org.zero3;
			headercrc		= org.headercrc;
		}
        public void ReadTableDefinition(FileStream fs)
		{	byte[]	array	= new byte[ headersize ];

			fs.Read( array, 0, headersize );
			TableName		= Convert.ToChar( array[3] ).ToString( ) + Convert.ToChar( array[2] ).ToString( ) + Convert.ToChar( array[1] ).ToString( ) + Convert.ToChar( array[0] ).ToString( );
			offsetFromIndex	= (((UInt32)array[7]) ) | (((UInt32)array[6]) << 8 ) | (((UInt32)array[5]) << 16 ) | (((UInt32)array[4]) << 24 );
		}
        public void ReadTableHeader(FileStream fs, long datastart)
		{	byte[]	array	= new byte[ infosize ];

        if (dataStart > fs.Length)
        {
            MessageBox.Show("fugz");
        }
			fs.Position	= datastart + offsetFromIndex;
			fs.Read( array, 0, infosize );

			priorcrc	= ( ((UInt32)array[ 3]) ) | ( ((UInt32)array[ 2]) << 8 ) | ( ((UInt32)array[ 1]) << 16 ) | ( ((UInt32)array[ 0]) << 24 );
			unknown_2	= ( ((UInt32)array[ 7]) ) | ( ((UInt32)array[ 6]) << 8 ) | ( ((UInt32)array[ 5]) << 16 ) | ( ((UInt32)array[ 4]) << 24 );
			len_bytes	= ( ((UInt32)array[11]) ) | ( ((UInt32)array[10]) << 8 ) | ( ((UInt32)array[ 9]) << 16 ) | ( ((UInt32)array[ 8]) << 24 );
			len_bits	= ( ((UInt32)array[15]) ) | ( ((UInt32)array[14]) << 8 ) | ( ((UInt32)array[13]) << 16 ) | ( ((UInt32)array[12]) << 24 );
			zero		= ( ((UInt32)array[19]) ) | ( ((UInt32)array[18]) << 8 ) | ( ((UInt32)array[17]) << 16 ) | ( ((UInt32)array[16]) << 24 );
			maxrecords	= (UInt16) (( ((UInt16)array[21]) ) | ( ((UInt16)array[20]) << 8 ));
			currecords	= (UInt16) (( ((UInt16)array[23]) ) | ( ((UInt16)array[22]) << 8 ));
			unknown_3	= ( ((UInt32)array[27]) ) | ( ((UInt32)array[26]) << 8 ) | ( ((UInt32)array[25]) << 16 ) | ( ((UInt32)array[24]) << 24 );
			numfields	= array[28];
			indexcount	= array[29];
			zero2		= (UInt16) (( ((UInt16)array[31]) ) | ( ((UInt16)array[30]) << 8 ));
			zero3		= ( ((UInt32)array[35]) ) | ( ((UInt32)array[34]) << 8 ) | ( ((UInt32)array[33]) << 16 ) | ( ((UInt32)array[32]) << 24 );
			headercrc	= ( ((UInt32)array[39]) ) | ( ((UInt32)array[38]) << 8 ) | ( ((UInt32)array[37]) << 16 ) | ( ((UInt32)array[36]) << 24 );

			fieldStart	= datastart + offsetFromIndex + infosize;
			dataStart	= fieldStart + ( numfields * 16 );

			DB_CRC	db	= new DB_CRC( );
			calcHcrc	= ~db.crc32_be( 0, array, (uint) infosize -8, 4 );
		}
		public void WriteHeader( byte[] buf )
		{
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x00 ), priorcrc );
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x04 ), unknown_2 );
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x08 ), len_bytes );
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x0C ), len_bits );
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x10 ), zero );
			DBFileInfo.WriteW2Buf(  buf, (int)( fieldStart - infosize +0x14 ), maxrecords );
			DBFileInfo.WriteW2Buf(  buf, (int)( fieldStart - infosize +0x16 ), currecords );
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x18 ), unknown_3 );
			buf[fieldStart - infosize +0x1C]	= numfields;
			buf[fieldStart - infosize +0x1D]	= indexcount;
			DBFileInfo.WriteW2Buf(  buf, (int)( fieldStart - infosize +0x1E ), zero2 );
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x20 ), zero3 );
			DBFileInfo.WriteDW2Buf( buf, (int)( fieldStart - infosize +0x24 ), headercrc );
		}
	}
	public class DBFileInfo
	{
#region internals & statics
		public UInt32		tableIndexOffset	= 0x24;		// from DB start ( really 20, but we're treating it differently )
		public byte[]		theFile				= null;
		public long			absPosition			= 0;
		public long			startData			= 0;

		public UInt16		header;
		public UInt16		version;
		public UInt32		unknown_1;
		public UInt32		DBsize;
		public UInt32		zero;
		public UInt32		tableCount;
		public UInt32		unknown_2;						// checksum on the header

		public int			headersize		= 24;			// really 20...

		public UInt32		calcdHeaderCRC		= 0;
		public UInt32		calcdEOFCRC			= 0;

		public List<DBTable>	lTables;

#endregion
		public DBFileInfo( )
		{
			header		= 0;
			version		= 0;
			unknown_1	= 0;
			DBsize		= 0;
			zero		= 0;
			tableCount	= 0;
			unknown_2	= 0;
			lTables		= new List<DBTable>( );
		}
        public DBFileInfo(FileStream fs)
		{
			header		= 0;
			version		= 0;
			unknown_1	= 0;
			DBsize		= 0;
			zero		= 0;
			tableCount	= 0;
			unknown_2	= 0;
			lTables		= new List<DBTable>( );

			ReadDBHeader( fs );
		}

        public  int? headerOffset = null; 
        public  int HeaderOffset { get { return headerOffset.Value; } }

        public void ReadDBHeader(FileStream fs)
		{	byte[]	array	= new byte[ headersize ];

			// first, load the whole file into memory
			fs.Position	= 0;
			theFile		= new byte[ fs.Length ];
			fs.Read( theFile, 0, (int) fs.Length );

			// now reset the file. already wrote the code to read all data in via the file, so keeping that, but will change writes to in-memory
			absPosition	= 0;
			fs.Position	= 0;
			fs.Read( array, 0, 4 );

			fs.Position	= absPosition;
			fs.Read( array, 0, headersize );

            for (int i = 0; i < theFile.Length; i++)
            {
                if (theFile[i] == 0x44 && theFile[i + 1] == 0x42)
                {
                    Array.Copy(theFile, i, array, 0, array.Length);
                    fs.Position = i + headersize;

                    if (headerOffset.HasValue == false) { headerOffset = i; }
                    break;
                }
            }

			header	= (UInt16) (( ((UInt16)array[1]) ) | ( ((UInt16)array[0]) << 8 ));
			version	= (UInt16) (( ((UInt16)array[3]) ) | ( ((UInt16)array[2]) << 8 ));

			unknown_1	= ( ((UInt32)array[ 7]) ) | ( ((UInt32)array[ 6]) << 8 ) | ( ((UInt32)array[ 5]) << 16 ) | ( ((UInt32)array[ 4]) << 24 );
			DBsize		= ( ((UInt32)array[11]) ) | ( ((UInt32)array[10]) << 8 ) | ( ((UInt32)array[ 9]) << 16 ) | ( ((UInt32)array[ 8]) << 24 );
			zero		= ( ((UInt32)array[15]) ) | ( ((UInt32)array[14]) << 8 ) | ( ((UInt32)array[13]) << 16 ) | ( ((UInt32)array[12]) << 24 );
			tableCount	= ( ((UInt32)array[19]) ) | ( ((UInt32)array[18]) << 8 ) | ( ((UInt32)array[17]) << 16 ) | ( ((UInt32)array[16]) << 24 );
			unknown_2	= ( ((UInt32)array[23]) ) | ( ((UInt32)array[22]) << 8 ) | ( ((UInt32)array[21]) << 16 ) | ( ((UInt32)array[20]) << 24 );
            TableNumber = tableCount;

			if( header == 0x4442 )	// 'DB'
			{
				for( int i=0; i< tableCount; i++ )		// read table index
					lTables.Add( new DBTable( fs ) );

				startData	= fs.Position;				// record start of actual data

                //read table info
                for (int i = 0; i < lTables.Count; i++)
                {
                    lTables[i].ReadTableHeader(fs, startData);
                }
			}

		}

        public static uint TableNumber = 0; 

		public void CalcChecksums( FileStream fs )
		{	DB_CRC	 db			= new DB_CRC( );
			UInt32	priorcrc	= 0;

			// first, load the whole file into memory
			fs.Position	= 0;
			theFile		= new byte[ fs.Length ];
			fs.Read( theFile, 0, (int) fs.Length );
			fs.Position	= 0;

			// DB header
			calcdHeaderCRC		= ~db.crc32_be( 0, theFile, 20, 0 );

			// Table index
			priorcrc	= ~db.crc32_be( 0, theFile, tableCount *8, 24 );

			// each table, minus 1
			for( long i=0; i < tableCount -1; i++ )
			{	long	start	= startData + lTables[ (int) i ].offsetFromIndex + lTables[ (int) i ].infosize;
				long	end		= startData + lTables[ (int) i +1 ].offsetFromIndex;

				lTables[ (int) i ].calcPcrc	= priorcrc;
				priorcrc		= ~db.crc32_be( 0, theFile, (uint)( end - start ), (uint) start );
			}
			// the last table
			long laststart	= startData + lTables[ (int)( tableCount -1 ) ].offsetFromIndex + lTables[ (int)( tableCount -1 ) ].infosize;
			long lastend	= DBsize -4;

			lTables[ (int)( tableCount -1 ) ].calcPcrc	= priorcrc;
			calcdEOFCRC		= ~db.crc32_be( 0, theFile, (uint)( lastend - laststart ), (uint) laststart );


			// put the data back into the buffer
			WriteDW2Buf( theFile, 20, calcdHeaderCRC );
			foreach( DBTable dt in lTables )
			{
				WriteDW2Buf( theFile, (int)(startData + dt.offsetFromIndex), dt.calcPcrc );
				WriteDW2Buf( theFile, (int)(startData + dt.offsetFromIndex + dt.infosize -4), dt.calcHcrc );
			}
			WriteDW2Buf( theFile, (int)(DBsize -4), calcdEOFCRC );

		}
		public void CalcChecksums( )
		{	DB_CRC	 db			= new DB_CRC( );
			UInt32	priorcrc	= 0;
			long	start		= 0;
			long	end			= 0;
            uint headerOffset = (uint)this.HeaderOffset;


			// DB header
			calcdHeaderCRC		= ~db.crc32_be( 0, theFile, 20, 0+headerOffset );

			// Table index
			priorcrc	= ~db.crc32_be( 0, theFile, tableCount *8, 24 + headerOffset);

			// each table, minus 1
			for( long i=0; i < tableCount -1; i++ )
			{
				// table header crc
				start							= startData + lTables[ (int) i ].offsetFromIndex +4;
				lTables[ (int) i ].calcHcrc		= ~db.crc32_be( 0, theFile, (uint)( lTables[ (int) i ].infosize -8 ), (uint) start );

				// table data crc
				start							= startData + lTables[ (int) i ].offsetFromIndex + lTables[ (int) i ].infosize;
				end								= startData + lTables[ (int) i +1 ].offsetFromIndex;

				lTables[ (int) i ].calcPcrc		= priorcrc;
				priorcrc						= ~db.crc32_be( 0, theFile, (uint)( end - start ), (uint) start );
			}

			// the last table
			// table header crc
			start											= startData + lTables[ (int)( tableCount -1 ) ].offsetFromIndex +4;
			lTables[ (int)( tableCount -1 ) ].calcHcrc		= ~db.crc32_be( 0, theFile, (uint)( lTables[ (int)( tableCount -1 ) ].infosize -8 ), (uint) start );

			// table data crc
			start	= startData + lTables[ (int)( tableCount -1 ) ].offsetFromIndex + lTables[ (int)( tableCount -1 ) ].infosize;
			end		=headerOffset+ DBsize -4;

			lTables[ (int)( tableCount -1 ) ].calcPcrc	= priorcrc;
			calcdEOFCRC		= ~db.crc32_be( 0, theFile, (uint)( end - start ), (uint) start );


			// put the data back into the buffer
			WriteDW2Buf( theFile, (int)headerOffset + 20, calcdHeaderCRC );
			foreach( DBTable dt in lTables )
			{
				WriteDW2Buf( theFile, (int)(startData + dt.offsetFromIndex), dt.calcPcrc );
				WriteDW2Buf( theFile, (int)(startData + dt.offsetFromIndex + dt.infosize -4), dt.calcHcrc );
			}
			WriteDW2Buf( theFile, (int)(headerOffset + DBsize -4), calcdEOFCRC );

		}
		public void Save( FileStream fs , bool saveWholeFile)
		{	fs.Position	= 0;
			CalcChecksums( );

            if (saveWholeFile)
            {
                fs.Write(theFile, 0, theFile.Length);
            }
            else
            {
                fs.Write(theFile, 0, (int)DBsize);
            }
		}

		/// <summary>
		/// used in the franchise -> roster conversion
		/// assumes theFile is already allocated
		/// </summary>
		public void DBHeaderToBuffer( )
		{
			WriteW2Buf(  theFile,  0, header );
			WriteW2Buf(  theFile,  2, version );
			WriteDW2Buf( theFile,  4, unknown_1 );
			WriteDW2Buf( theFile,  8, DBsize );
			WriteDW2Buf( theFile, 12, zero );
			WriteDW2Buf( theFile, 16, tableCount );
			WriteDW2Buf( theFile, 20, unknown_2 );
		}

		public static void WriteDW2Buf( byte[] buf, int offset, UInt32 dword )
		{
			buf[offset +0]	= (byte)( (dword >> 24) & 0x000000FF );
			buf[offset +1]	= (byte)( (dword >> 16) & 0x000000FF );
			buf[offset +2]	= (byte)( (dword >>  8) & 0x000000FF );
			buf[offset +3]	= (byte)( (dword      ) & 0x000000FF );
		}
		public static void WriteW2Buf( byte[] buf, int offset, UInt16 word )
		{
			buf[offset +0]	= (byte)( (word >>  8) & 0x00FF );
			buf[offset +1]	= (byte)( (word      ) & 0x00FF );
		}
		public static void WriteBytesFromBuf( byte[] des, byte[] src, long offset, int bytes )
		{
			for( int i=0; i < bytes; i++ )
				des[ i ]	= src[ i +offset ];
		}
		public static void WriteBytesToBuf( byte[] des, byte[] src, long offset, int bytes )
		{
			for( int i=0; i < bytes; i++ )
				des[ i +offset ]	= src[ i ];
		}
	}
	public class DB_CRC
	{
		private	UInt32		CRCPOLY_BE		= 0x04c11db7;
		private	UInt32[]	crc32table_be	= new UInt32 [256];

		public DB_CRC( )
		{	crc32init_be( );
		}
		public DB_CRC( UInt32 poly )
		{	CRCPOLY_BE	= poly;
			crc32init_be( );
		}
		private void crc32init_be( )
		{
			UInt32	i, j;
			UInt32	crc		= 0x80000000;

			crc32table_be[0] = 0;

			for( i = 1 ; i < 1<< 4; i <<= 1)
			{
				crc	= (crc << 1) ^ ( ((crc & 0x80000000) != 0) ? CRCPOLY_BE : 0);
				for( j = 0; j < i; j++ )
					crc32table_be[ i+j ]	= crc ^ crc32table_be[ j ];
			}
		}
		public UInt32 crc32_be( UInt32 crc, byte[] p, UInt32 len, UInt32 start=0 )
		{	UInt32	x	= start;

			crc ^= 0xFFFFFFFF;
			while( len-- >0 )
			{
				crc	^= (uint) p[ x++ ] << 24;
				crc	= (crc << 4) ^ crc32table_be[crc >> 28];
				crc	= (crc << 4) ^ crc32table_be[crc >> 28];
			}
			return crc ^ 0xFFFFFFFF;
		}
	}
	public class MC02Descriptor
	{
		public byte[]	data;	

		public MC02Descriptor( )
		{	data	= new byte[ 40 ];

			SetDword( 16, 0x524c5f50 );
			SetDword( 20, 0x61746368 );
			SetDword( 24, 0x322d3731 );
			SetDword( 28, 0x35303032 );
			SetDword( 32, 0x00000000 );
			SetDword( 36, 0x00000000 );
		
			Offset	= 0x000b15dc;
			Year	= 2012;
			Month	= 1;
			Day		= 1;
			Hour	= 12;
			Minute	= 0;
			Second	= 0;
		}

		private UInt16 GetWord( int index )
		{
			return	(UInt16) ( (( Convert.ToUInt16( data[index] ) & 0x00ff ) << 8 ) | ( Convert.ToUInt16( data[index+1] ) & 0x00ff ));
		}
		private UInt32 GetDword( int index )
		{
			return ( Convert.ToUInt32( GetWord(index) & 0x0000ffff ) << 16 ) | ( Convert.ToUInt32( GetWord(index+2) ) & 0x0000ffff );
		}
		private void SetWord( int index, UInt16 word )
		{
			data[index +0]	= (byte)( (word >>  8) & 0x00FF );
			data[index +1]	= (byte)( (word      ) & 0x00FF );
		}
		private void SetDword( int index, UInt32 dword )
		{
			SetWord( index +0, (UInt16)( (dword >> 16) & 0x0000FFFF ) );
			SetWord( index +2, (UInt16)( (dword      ) & 0x0000FFFF ) );
		}

		public UInt32 Offset
		{
			get
			{	return GetDword( 0 );
			}
			set
			{	SetDword( 0, value );
			}
		}
		public UInt16 Year
		{
			get
			{	return GetWord( 4 );
			}
			set
			{	SetWord( 4, value );
			}
		}
		public UInt16 Month
		{
			get
			{	return GetWord( 6 );
			}
			set
			{	SetWord( 6, value );
			}
		}
		public UInt16 Day
		{
			get
			{	return GetWord( 8 );
			}
			set
			{	SetWord( 8, value );
			}
		}
		public UInt16 Hour
		{
			get
			{	return GetWord( 10 );
			}
			set
			{	SetWord( 10, value );
			}
		}
		public UInt16 Minute
		{
			get
			{	return GetWord( 12 );
			}
			set
			{	SetWord( 12, value );
			}
		}
		public UInt16 Second
		{
			get
			{	return GetWord( 14 );
			}
			set
			{	SetWord( 14, value );
			}
		}
	}
	public class MaddenRecord
	{
		public MaddenTable		Table			= null;
		public List<DBData>		lEntries		= null;

		public MaddenRecord( )
		{	lEntries	= new List<DBData>( );
		}
		public MaddenRecord( MaddenTable table, List<Field> lFields )
		{	BitStream	bits	= new BitStream( );

			Table	= table;
			bits.AllocBits( Table.Table.len_bytes *8 );
			bits.ActivateBits( );
			
			lEntries	= new List<DBData>( );

			foreach( Field f in lFields )
			{	DBData	db	= new DBData( f );
				lEntries.Add( db );
			}
		}
        public MaddenRecord(int entryNum, MaddenTable table, List<Field> lFields, FileStream fs)
		{	BitStream	bits	= new BitStream( );

			Table	= table;
			bits.AllocBits( Table.Table.len_bytes *8 );
			fs.Position	= Table.Table.dataStart + ( entryNum * Table.Table.len_bytes );
			fs.Read( bits.buffer, 0, (int) Table.Table.len_bytes );
			bits.ActivateBits( );
			
			lEntries	= new List<DBData>( );

			foreach( Field f in lFields )
			{	DBData	db	= new DBData( f, bits );
				lEntries.Add( db );
			}
		}
		public MaddenRecord( MaddenRecord org )
		{	Table		= org.Table;
			lEntries	= new List<DBData>( );
			foreach( DBData db in org.lEntries )
			{	DBData	n	= new DBData( db.field );
				//n.Data		= db.Data;
				lEntries.Add( n );
			}
		}
		public MaddenRecord( MaddenRecord org, List<Field> lNewFields )
		{	Table		= org.Table;
			lEntries	= new List<DBData>( );
			foreach( Field f in lNewFields )
			{	DBData	n	= new DBData( f );
				DBData	r	= org.GetEntry( f.name );
				if( r != null )
					n.Data	= r.Data;
				lEntries.Add( n );
			}
		}
		public void CopyData( MaddenRecord org )
		{	if( org.lEntries.Count != lEntries.Count )
				return;

			for( int i=0; i < lEntries.Count; i++ )
				lEntries[i].Data	= org.lEntries[i].Data;
		}
		public void WriteRecord( int entryNum, byte[] buffer, bool isScheduleTable )
		{	BitStream	bits	= new BitStream( );

			bits.AllocBits( Table.Table.len_bytes *8 );

			long	position	= Table.Table.dataStart + ( entryNum * Table.Table.len_bytes );
			DBFileInfo.WriteBytesFromBuf( bits.buffer, buffer, position, (int) Table.Table.len_bytes );

			bits.ActivateBits( );
			
			foreach( DBData db in lEntries )
				db.WriteData( bits );

			bits.DeactivateBits( );

            if(isScheduleTable)
            {
                bits.buffer[19] = 0;
            }

			position	= Table.Table.dataStart + ( entryNum * Table.Table.len_bytes );
			DBFileInfo.WriteBytesToBuf( buffer, bits.buffer, position, (int) Table.Table.len_bytes );
		}
		public DBData GetEntry( string fieldName )
		{	foreach( DBData data in lEntries )
			{	if( data.GetFieldName( ) == fieldName )
					return data;
			}
			return null;
		}
		public string this[ string fieldName ]
		{
			get
			{	DBData	data	= GetEntry( fieldName );
				if( data == null )
					return "0";
				return data.Data;
			}
			set
			{	DBData	data	= GetEntry( fieldName );
				if( data != null )
					data.Data	= value;
			}
		}
	}
    public class MaddenTable
    {
        public DBTable Table = null;
        public List<Field> lFields = null;
        public List<MaddenRecord> lRecords = null;

        // for experimental XML mapping
        public string Abbreviation = "";
        public string Name = "";


        public MaddenTable(DBTable table, FileStream fs)
        {
            Table = table;
            Abbreviation = table.TableName;
            lFields = Field.ReadFields(Table, fs);
            lRecords = new List<MaddenRecord>();

            for (int i = 0; i < Table.currecords; i++)
            {
                MaddenRecord mr = new MaddenRecord(i, this, lFields, fs);
                lRecords.Add(mr);
            }

            if (table.offsetFromIndex > fs.Length)
            {
                MessageBox.Show("fuck");
            }
        }

        public MaddenTable(MaddenTable table)
        {
            Table = new DBTable(table.Table);
            lFields = table.lFields;
            lRecords = new List<MaddenRecord>();

            foreach (MaddenRecord mr in table.lRecords)
            {
                MaddenRecord mr2 = new MaddenRecord(mr);
                mr2.Table = this;
                mr2.CopyData(mr);
                lRecords.Add(mr2);
            }
        }
        public MaddenTable(MaddenTable table, List<Field> lNewFields)
        {
            Table = new DBTable(table.Table);
            lFields = lNewFields;
            lRecords = new List<MaddenRecord>();

            foreach (MaddenRecord mr in table.lRecords)
            {
                MaddenRecord mr2 = new MaddenRecord(mr, lNewFields);
                mr2.Table = this;
                lRecords.Add(mr2);
            }
        }
        public MaddenTable()
        {
        }

        public void Clear()
        {
            this.lRecords.Clear();
            this.Table.currecords = 0;
        }

        public override string ToString()
        {
            return Table.TableName;
        }
        public static int CompareName(MaddenTable x, MaddenTable y)
        {
            return x.ToString().CompareTo(y.ToString());
        }

        /// <summary>
        /// adds a record - to be used by wrapper class so that it can then set values appropriately
        /// </summary>
        public MaddenRecord AddNewRecord()
        {
            MaddenRecord mr = null;

            if (Table.currecords < Table.maxrecords)
            {
                mr = new MaddenRecord(this, lFields);

                if (mr != null)
                {
                    lRecords.Add(mr);
                    Table.currecords++;
                }
            }
            return mr;
        }
        public bool InsertRecord(MaddenRecord mr)
        {
            if (Table.currecords < Table.maxrecords)
            {
                lRecords.Add(mr);
                Table.currecords++;
                return true;
            }
            return false;
        }

        public bool RemoveRecord(MaddenRecord mr)
        {
            if(lRecords.Remove(mr))
            {
                Table.currecords--;
                return true;
            }

            return false;
        }


        /// <summary>
        /// write a back out to the file
        /// </summary>
        public void WriteTable(byte[] buffer)
        {
            Table.WriteHeader(buffer);

            int x = 0;
            foreach (Field field in lFields)
            {
                field.WriteField(buffer, (int)(Table.fieldStart + (x * 16)));
                x++;
            }

            x = 0;
            foreach (MaddenRecord mr in lRecords)
                mr.WriteRecord(x++, buffer,Table.TableName=="SCHD");
        }

        /// <summary>
        /// find a MaddenTable from a list of MaddenTables ( typicalling in the MaddenDatabase )
        /// </summary>
        public static MaddenTable FindMaddenTable(List<MaddenTable> lMaddenTables, string name)
        {
            foreach (MaddenTable mt in lMaddenTables)
            {
                if (mt.Table.TableName == name)
                    return mt;
            }
            return null;
        }

        public static MaddenTable GetTableByName(List<MaddenTable> lMaddenTables, string name)
        {
            foreach (MaddenTable t in lMaddenTables)
            {
                if (t.Name == name)
                    return t;
            }
            return null;
        }
        public static MaddenTable GetTableByAbbreviation(List<MaddenTable> lMaddenTables, string name)
        {
            foreach (MaddenTable t in lMaddenTables)
            {
                if (t.Abbreviation == name)
                    return t;
            }
            return null;
        }
        public static MaddenTable FindTable(List<MaddenTable> lMaddenTables, string Name)
        {
            MaddenTable t = GetTableByName(lMaddenTables, Name);
            if (t != null)
                return t;
            return GetTableByAbbreviation(lMaddenTables, Name);
        }

        public static List<MaddenRecord> Query(List<MaddenTable> lMaddenTables,string table, Dictionary<string, string> kvp)
        {
            return Query(FindTable(lMaddenTables, table), kvp);
        }

        public static List<MaddenRecord> Query(MaddenTable table, Dictionary<string, string> kvp)
        {
            var records = new List<MaddenRecord>();
            foreach (var record in table.lRecords)
            {
                bool isMatch = true;
                foreach (var key in kvp.Keys)
                {
                    isMatch &= record[key] == kvp[key];
                    if (!isMatch)
                        break;
                }

                if (isMatch)
                    records.Add(record);
            }

            return records;
        }
    }

	public class MaddenDatabase
	{	public	DBFileInfo			dbFileInfo		= null;
		public	List<MaddenTable>	lTables			= new List<MaddenTable>( );
		public	string				fileName		= "";
		public	string				realfileName	= "";
		public	MaddenFileType		type			= MaddenFileType.FileType_None;
		

		private MaddenDatabase( )
		{	dbFileInfo	= new DBFileInfo( );
		}
        public MaddenDatabase(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            realfileName = file;

            // check file type
            type = MaddenDatabase.CheckFileType(fs);

#region unknown file type
            if (type == MaddenFileType.FileType_None)
            {
                MessageBox.Show("Error - this is not a DB or MC02 file!");
                fs.Close();
                return;
            }
#endregion
#region CON file type
            if (type == MaddenFileType.FileType_CON)
            {
                MessageBox.Show("You must first extract the MC02 / DB file from the roster!");
                fs.Close();
                return;
            }
#endregion
#region MC02 file type
            if (type == MaddenFileType.FileType_MC02 )
            {
                MC02Handler.Package package = null;
                byte[] mc02 = new byte[fs.Length];

                try
                {
                    fs.Read(mc02, 0, (int)fs.Length);
                    fs.Position = 0;
                    package = new MC02Handler.Package(mc02);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Error opening MC02 package: " + exception.ToString());
                    fs.Close();
                    return;
                }

                // extract the DB file now & set the filename to work on
                fileName = realfileName + ".DB";
                package.Extract(Package.DataType.SaveData, fileName);
                package.Dispose();

                fs.Close();
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            }
            else    // must be a DB file
#endregion
#region DB file type
                fileName = file;
#endregion

            dbFileInfo = new DBFileInfo(fs);
            for (int i = 0; i < dbFileInfo.lTables.Count; i++)
            {
                lTables.Add(new MaddenTable(dbFileInfo.lTables[i], fs));
            }

            fs.Close();
        }

        public MaddenTable GetTable( string tableName )
		{	foreach( MaddenTable table in lTables )
			{	if( table.Table.TableName == tableName )
					return table;
			}
			return null;
		}
		public MaddenTable this[ string tableName ]
		{
			get
			{	return GetTable( tableName );
			}
		}
		public void Save( )
		{
			FileStream	fs	= new FileStream( fileName, FileMode.Truncate, FileAccess.ReadWrite );

			foreach( MaddenTable mt in lTables )
				mt.WriteTable( dbFileInfo.theFile );

			dbFileInfo.Save( fs, type == MaddenFileType.FileType_DB );
			fs.Close( );

#region if this was an MC02 file, repackage
			if( type == MaddenDatabase.MaddenFileType.FileType_MC02 )
			{	fs	= new FileStream( realfileName, FileMode.Open, FileAccess.ReadWrite );

				// read the data descriptor @ 0x14 and save it
				byte[]	descriptor	= new byte[ 12 ];
				fs.Position			= 0x10;
				fs.Read( descriptor, 0, 12 );
				fs.Position			= 0;

				MC02Handler.Package	package	= null;
				byte[]	mc02				= new byte[ fs.Length ];

				try
				{	fs.Read( mc02, 0, (int) fs.Length );
					package	= new MC02Handler.Package( mc02 );

				} catch( Exception exception )
				{
					MessageBox.Show( "Error opening MC02 package to insert DB: " +exception.ToString( ) );
					Cursor.Current	= Cursors.Default;
					return;
				}

				package.Overwrite( Package.DataType.SaveData, dbFileInfo.theFile );
				mc02	= package.Save( true );

				// keep descriptor, ex. NCAA 14
				if( ! Form1.mc02Recalc )
				{
					mc02[ 0x10 ]	= descriptor[ 0 ];
					mc02[ 0x11 ]	= descriptor[ 1 ];
					mc02[ 0x12 ]	= descriptor[ 2 ];
					mc02[ 0x13 ]	= descriptor[ 3 ];
					mc02[ 0x14 ]	= descriptor[ 4 ];
					mc02[ 0x15 ]	= descriptor[ 5 ];
					mc02[ 0x16 ]	= descriptor[ 6 ];
					mc02[ 0x17 ]	= descriptor[ 7 ];
					mc02[ 0x18 ]	= descriptor[ 8 ];
					mc02[ 0x19 ]	= descriptor[ 9 ];
					mc02[ 0x1A ]	= descriptor[ 10 ];
					mc02[ 0x1B ]	= descriptor[ 11 ];
				}

				fs.Position	= 0;
				fs.Write( mc02, 0, mc02.Length );
				fs.Close( );

				package.Dispose( );
			}
#endregion
		}
		public void SaveAs( string newfile )
		{
			File.Copy( fileName, newfile + ".DB" );
			File.Copy( realfileName, newfile );

			fileName		= newfile + ".DB";
			realfileName	= newfile;

			Save( );
		}

		public enum MaddenFileType
		{	FileType_None,
			FileType_DB,
			FileType_MC02,
			FileType_CON,
        }
        public static MaddenFileType CheckFileType(FileStream fs)
        {
            byte[] b = new byte[4];

            fs.Position = 0;
            fs.Read(b, 0, 4);
            fs.Position = 0;


            // on ps3 a team builder save starts with 'DD'
            if (b[0] == 'D' && b[1] == 'B' )
                return MaddenFileType.FileType_DB;
            if ((b[0] == 'M' && b[1] == 'C' && b[2] == '0' && b[3] == '2') )
                return MaddenFileType.FileType_MC02;
            if (b[0] == 'C' && b[1] == 'O' && b[2] == 'N')
                return MaddenFileType.FileType_CON;
            return MaddenFileType.FileType_None;
        }
		/// <summary>
		/// create a new, blank, Madden 12 roster
		/// all of this is subject to change for future versions of Madden!
		/// </summary>
		public static MaddenDatabase CreateMaddeDB_Roster( )
		{	MaddenDatabase	md	= new MaddenDatabase( );

			// reserve our file space
			md.dbFileInfo.theFile		= new byte[ 0x000b15b4 ];

			// file in the typical DB header values for a roster ( as of Madden 12 )
			md.dbFileInfo.header		= 0x4442;
			md.dbFileInfo.version		= 0x0008;
			md.dbFileInfo.unknown_1		= 0x01000000;
			md.dbFileInfo.DBsize		= 0x000b15b4;
			md.dbFileInfo.zero			= 0x00000000;
			md.dbFileInfo.tableCount	= 0x00000004;
			md.dbFileInfo.unknown_2		= 0xe0dc10f5;
			md.dbFileInfo.DBHeaderToBuffer( );

			md.dbFileInfo.startData		= 56;

			// build our default table header
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 24, 0x54484344 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 28, 0x00000000 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 32, 0x594a4e49 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 36, 0x00005b68 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 40, 0x59414c50 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 44, 0x00006610 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 48, 0x4d414554 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 52, 0x000af730 );
			DBFileInfo.WriteDW2Buf( md.dbFileInfo.theFile, 56, 0x9dcf334f );

			return md;
		}
	}

	public class View
	{
		public delegate void ViewChanged( View v );
		public delegate Field GetMappedField( string name );
#region members
		public string					Name			= "";
		public string					Type			= "";
		public Control					DisplayControl	= null;
		public int						Position_x		= 0;
		public int						Position_y		= 0;
		public int						Position_z		= 0;
		public int						Size_width		= 0;
		public int						Size_height		= 0;
		public int						ChildCount		= 0;
		public List<string>				ChildViews		= new List<string>( );
		public List<View>				lChildren		= new List<View>( );
		public string					SourceType		= "";
		public string					SourceName		= "";
		public int						ChildFieldCount	= 0;
		public List<string>				ChildFields		= new List<string>( );
		public List<Field>				lChildFields	= new List<Field>( );
		public List<FieldFilter>		lastFilters		= null;
		public ToolTip					toolTip			= new ToolTip( );
		static public ViewChanged		viewChanged		= null;
		static public GetMappedField	getMappedField	= null;
        bool isRecruitPitchTable = false;
#endregion

		public View( )
		{
		}
		public override string ToString()
		{
			return Name;
		}
		public bool ProcessSettings( List<Field> lMappedFields )
		{
			switch( Type )
			{
#region grid list view
				case "Grid":
					// setup list view
					ListViewEx.ListViewEx	lv	= new ListViewEx.ListViewEx( );
					lv.FullRowSelect			= true;
					lv.DoubleClickActivation	= true;
					lv.Height					= Size_height;
					lv.Width					= Size_width;
					lv.Location					= new Point( Position_x, Position_y );
					lv.Anchor					= AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
					lv.ListClicked				+= new SubItemEventHandler( GridClick );
					lv.SubItemClicked			+= new SubItemEventHandler( GridItemClick );
					lv.SubItemEndEditing		+= new SubItemEndEditingEventHandler( GridItemEdited );
					lv.ColumnClick				+= new ColumnClickEventHandler( GridColumnClicked );
//					lv.SelectedIndexChanged		+= new EventHandler(lv_SelectedIndexChanged);

					// add the columns
					lv.Columns.Add( "" );
					for( int i=0; i < ChildFields.Count; i++ )
					{
						Field	f	= Field.FindField( lMappedFields, ChildFields[i] );
						if( f == null )
						{	MessageBox.Show( "Field " + ChildFields[i] + " in view " + Name + " not in main field list; aborting" );
							return false;
						}
						lChildFields.Add( f );

						string	field	= ( f.Name != "" ) ? f.Name : f.Abbreviation;

						ColumnHeader	ch	= new ColumnHeader( );
						ch.Text				= field;
						ch.Tag				= f;

						lv.Columns.Add( ch );
					}
					lv.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );

					// save the control
					DisplayControl				= lv;
					DisplayControl.Tag			= this;
					break;
#endregion

#region list item list view
				case "List Item":
					// setup list view ( this is a name / value type view )
					ListViewEx.ListViewEx	lv2	= new ListViewEx.ListViewEx( );
					lv2.FullRowSelect			= true;
					lv2.DoubleClickActivation	= true;
					lv2.Height					= Size_height;
					lv2.Width					= Size_width;
					lv2.Location				= new Point( Position_x, Position_y );

					// add two columns
					lv2.Columns.Add( "Field" );
					lv2.Columns.Add( "Value" );
					lv2.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );

					// save the control
					DisplayControl				= lv2;
					DisplayControl.Tag			= this;
					break;
#endregion

#region tab view
				case "Tab":	// to do
					TabControl	tab				= new TabControl( );
					tab.Height					= Size_height;
					tab.Width					= Size_width;
					tab.Location				= new Point( Position_x, Position_y );
					tab.Anchor					= AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
					tab.Selecting				+= new TabControlCancelEventHandler( TabSelecting );

					DisplayControl				= tab;
					DisplayControl.Tag			= this;
					break;
#endregion
			}
			return true;
		}

		public void UpdateGridData( MaddenTable maddenTable, List<FieldFilter> lFilters=null )
		{	int	i, j;
        MaddenTable table;
        Dictionary<int, int> recruits = new Dictionary<int, int>();

        if (maddenTable.Abbreviation == "RCPR")
        {
            isRecruitPitchTable = true;
            table= MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");
           foreach (var record in table.lRecords)
           {
               int recruitId = 0;
               int recruitRank = 0;
               foreach (var entry in record.lEntries)
               {
                   if (entry.field.Abbreviation == "PRSI")
                   {
                       recruitId = Int32.Parse(entry.Data);
                   }
                   else if (entry.field.Abbreviation == "RCRK")
                   {
                       recruitRank = Int32.Parse(entry.Data);
                   }
               }
               recruits.Add(recruitId, recruitRank);
           }
        }

			Cursor.Current	= Cursors.WaitCursor;
			((ListView) DisplayControl).BeginUpdate( );

			lastFilters	= lFilters;
#region columns specified
			if( ChildFields.Count > 0 )
			{
				((ListView) DisplayControl).Items.Clear( );

				for( i=0; i < maddenTable.lRecords.Count; i++ )
				{
					if( ! FieldFilter.ProcessFilters( lFilters, lChildFields, maddenTable.lRecords[ i ] ) )
						continue;

					ListViewItem lvitems = new ListViewItem( ( i+1 ).ToString( ) );

					for( j=0; j < ChildFields.Count; j++ )
					{
						Field							f	= Field.FindField( lChildFields, ChildFields[ j ] );
						ListViewItem.ListViewSubItem	sub	= null;

						switch( f.ControlType )
						{
							case "TextBox":
                                if (isRecruitPitchTable && j==0)
                                {
                                    var recruitRankField = maddenTable.lRecords[i].lEntries.Where(entry => entry.field.Abbreviation == "PRSI").SingleOrDefault();
                                    var rid = Int32.Parse(recruitRankField.Data);
                                    sub = new ListViewItem.ListViewSubItem(lvitems, recruits[rid].ToString());
                                }
                                else
                                {
                                    sub = new ListViewItem.ListViewSubItem(lvitems, maddenTable.lRecords[i][f.Abbreviation]);
                                }
								break;

							case "ComboBox":

								if( f.ControlLink != "" )
								{
#region select the item via lookup
#region find the ojbect in the list
									if( f.KeyToIndexMappings.ContainsKey( maddenTable.lRecords[ i ][ f.Abbreviation ] ) )
									{	((ComboBox) f.EditControl).SelectedIndex	= f.KeyToIndexMappings[ maddenTable.lRecords[ i ][ f.Abbreviation ] ];
										sub	= new ListViewItem.ListViewSubItem( lvitems, ((ComboBox) f.EditControl).SelectedItem.ToString( ) );
									}

									//for( x=0; x < ((ComboBox) f.EditControl).Items.Count; x++ )
									//{
									//    RefObj	ro	= (RefObj) ((ComboBox) f.EditControl).Items[ x ];

									//    if( ro.key == maddenTable.lRecords[ i ][ f.Abbreviation ] )
									//    {	((ComboBox) f.EditControl).SelectedIndex	= x;
									//        break;
									//    }
									//}
#endregion
									//if( ((ComboBox) f.EditControl).SelectedIndex > -1 )
									//    sub	= new ListViewItem.ListViewSubItem( lvitems, ((ComboBox) f.EditControl).SelectedItem.ToString( ) );
									else
									{
										RefObj	ro	= new RefObj( maddenTable.lRecords[ i ][ f.Abbreviation ], maddenTable.lRecords[ i ][ f.Abbreviation ] );
										((ComboBox) f.EditControl).Items.Add( ro );
										f.KeyToIndexMappings.Add( maddenTable.lRecords[ i ][ f.Abbreviation ], ((ComboBox) f.EditControl).Items.Count -1 );
										((ComboBox) f.EditControl).SelectedIndex	= ((ComboBox) f.EditControl).Items.Count -1;
										sub	= new ListViewItem.ListViewSubItem( lvitems, ro.value );
									}
#endregion

								}	else
								{
#region select by index
                                    if (((ComboBox)f.EditControl).Items.Count < Convert.ToInt32(maddenTable.lRecords[i][f.Abbreviation]))
                                        sub = new ListViewItem.ListViewSubItem(lvitems, maddenTable.lRecords[i][f.Abbreviation]);
                                    else
                                    {
                                        ((ComboBox)f.EditControl).SelectedIndex = Math.Min(((ComboBox)f.EditControl).Items.Count - 1, Convert.ToInt32(maddenTable.lRecords[i][f.Abbreviation]));
                                        sub = new ListViewItem.ListViewSubItem(lvitems, ((ComboBox)f.EditControl).SelectedItem.ToString());
                                    }
#endregion
								}
								break;

							case "Calculated":
								sub	= new ListViewItem.ListViewSubItem( lvitems, f.RunFormula( lChildFields, maddenTable.lRecords[ i ] ).ToString( ) );
								break;

							case "AdjustedComboBox":
								((ComboBox) f.EditControl).SelectedIndex	= Convert.ToInt32( maddenTable.lRecords[ i ][ f.Abbreviation ] ) + f.Offset;
								sub	= new ListViewItem.ListViewSubItem( lvitems, ((ComboBox) f.EditControl).SelectedItem.ToString( ) );
								break;

							case "MappedComboBox":
								Field	f2	= ( getMappedField != null ) ? getMappedField( f.ControlIF ) : null;
								if( f2 == null )
								{	MessageBox.Show( "Could not find mapped field therefore cannot edit value!" );
									break;
								}
								((ComboBox) f.EditControl).SelectedIndex	= Convert.ToInt32( maddenTable.lRecords[ i ][ f2.Abbreviation ] );
								sub	= new ListViewItem.ListViewSubItem( lvitems, ((ComboBox) f.EditControl).SelectedItem.ToString( ) );
								break;

							case "TimeOfDayInMinutes":
								TimeSpan	span	= TimeSpan.FromMinutes( Convert.ToInt32( maddenTable.lRecords[ i ][ f.Abbreviation ] ) );
								DateTime	time	= new DateTime( 2012, 1, 1 );
								time				= time + span;
								sub					= new ListViewItem.ListViewSubItem( lvitems, time.ToString( "t" ) );
								break;

							default:
								sub	= new ListViewItem.ListViewSubItem( lvitems, maddenTable.lRecords[ i ][ f.Abbreviation ] );
								break;
						}
						sub.Tag								= f;
						lvitems.SubItems.Add( sub );
					}

					lvitems.UseItemStyleForSubItems	= true;
					lvitems.Tag						= maddenTable.lRecords[ i ];

					((ListView) DisplayControl).Items.Add( lvitems );

				}
			}
#endregion
#region columns not specified
			else
			{
				((ListView) DisplayControl).Clear( );

				for( i=0; i < maddenTable.lFields.Count; i++ )
				{
					ColumnHeader	ch	= new ColumnHeader( );
					ch.Text				= maddenTable.lFields[ i ].name;
					ch.Tag				= maddenTable.lFields[ i ];

					((ListView) DisplayControl).Columns.Add( ch );
				}

				for( i=0; i < maddenTable.lRecords.Count; i++ )
				{
					if( ! FieldFilter.ProcessFilters( lFilters, maddenTable.lFields, maddenTable.lRecords[ i ] ) )
						continue;

					ListViewItem lvitems = new ListViewItem( ( i+1 ).ToString( ) );

					for( j=0; j < maddenTable.lFields.Count; j++ )
					{
						ListViewItem.ListViewSubItem	sub	= new ListViewItem.ListViewSubItem( lvitems, maddenTable.lRecords[ i ][ maddenTable.lFields[ j ].name ] );
						sub.Tag								= maddenTable.lFields[ j ];
						lvitems.SubItems.Add( sub );
					}

					lvitems.UseItemStyleForSubItems	= true;
					lvitems.Tag						= maddenTable.lRecords[ i ];

					((ListView) DisplayControl).Items.Add( lvitems );

				}
			}
#endregion

			((ListView) DisplayControl).AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
			((ListView) DisplayControl).EndUpdate( );
			Cursor.Current	= Cursors.Default;
		}
		public void RefreshGridData( MaddenTable maddenTable )
		{
			UpdateGridData( maddenTable, lastFilters );
		}

        public void SortForSchedule()
        {
            var comparer = ScheduleComparer.Instance;
            ((ListView)DisplayControl).ListViewItemSorter = comparer;
            ((ListView)DisplayControl).Sort();

            ListViewItemComparer.sortDir = -ListViewItemComparer.sortDir;
        }

        private static HashSet<string> AllowedSingleClickEdits = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SEWN",
            "GHTG",
            "GATG",
        };

		public void GridClick( object obj, SubItemEventArgs args )
		{
			Field	f	= (Field) args.Item.SubItems[ args.SubItem ].Tag;
			if( f != null && f.EditControl != null && ! f.ControlLocked )
			{
				if( (args.Button & MouseButtons.Right) != 0 )
				{
					Point	point	= DisplayControl.PointToClient( Cursor.Position );
					if( f.Description != "" )
						toolTip.Show( f.Description, DisplayControl, point.X, point.Y, 5000 );
				}
			}

            args.SingleClickAllowed = AllowedSingleClickEdits.Contains(f.Abbreviation);
		}

		public void GridItemClick( object obj, SubItemEventArgs args )
		{
			Field	f	= (Field) args.Item.SubItems[ args.SubItem ].Tag;
			if( f != null && f.EditControl != null && ! f.ControlLocked && f.ControlType != "Calculated" )
			{
				f.EditControl.Parent	= DisplayControl;
				((ListViewEx.ListViewEx) DisplayControl).StartEditing( f.EditControl, args.Item, args.SubItem );
			}
		}
		public void GridItemEdited( object obj, SubItemEventArgs args )
		{
            if (this.isRecruitPitchTable && args.SubItem == 1)
            {
                return;
            }

			Field	f	= Field.FindField( lChildFields, ((ListView) DisplayControl).Columns[ args.SubItem ].Text );
			if( f != null && f.EditControl != null )
			{
				MaddenRecord	mr	= (MaddenRecord) args.Item.Tag;

				if( mr != null )
				{
                    switch (f.ControlType)
                    {
                        case "ComboBox":
                            if (f.ControlLink != "")
                            {	// set via lookup
                                if (((ComboBox)f.EditControl).SelectedIndex > -1)
                                {
                                    string previousValue = mr[f.Abbreviation];
                                    RefObj ro = (RefObj)((ComboBox)f.EditControl).SelectedItem;
                                    mr[f.Abbreviation] = ro.key;


                                    // modifying game location
                                    if (mr.Table.Abbreviation == "SCHD" && f.Abbreviation == "SGID")
                                    {
                                        // get the game # and week # and home/away teams
                                        var gameNum = mr["SGNM"];
                                        var weekNum = mr["SEWN"];
                                        var homeTeam = mr["GHTG"];
                                        var awayTeam = mr["GATG"];

                                        var query = new Dictionary<string, string>();
                                        query["SGNM"] = gameNum;
                                        query["SEWN"] = weekNum;

                                        // get the team schedule
                                        var teamScheduleTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TSCH");
                                        query["TGID"] = homeTeam;
                                        var teamScheduleRecord = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
                                        query["TGID"] = awayTeam;
                                        var oppRecord = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
                                        teamScheduleRecord["THOA"] = "1";
                                        oppRecord["THOA"] = "1";
                                    }

                                    // if modifying the schedule, modify team schedule
                                    else if (mr.Table.Abbreviation == "SCHD" && !Form1.PreseasonScheduleEdit)
                                    {
                                        // get the team schedule
                                        var teamScheduleTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TSCH");

                                        // get the game # and week #
                                        var gameNum = mr["SGNM"];
                                        var weekNum = mr["SEWN"];

                                        var query = new Dictionary<string, string>();
                                        query["SGNM"] = gameNum;
                                        query["SEWN"] = weekNum;
                                        query["TGID"] = previousValue;

                                        //find stadium id
                                        var teamQuery = new Dictionary<string, string>();
                                        teamQuery["TGID"] = ro.key;
                                        var teamRecord = MaddenTable.Query(Form1.MainForm.maddenDB.lTables, "TEAM", teamQuery).SingleOrDefault();
                                        var teamStadium = teamRecord["SGID"];

                                        var teamScheduleRecord = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
                                        query.Remove("TGID");
                                        query["OGID"] = previousValue;
                                        var oppRecord = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
                                        teamScheduleRecord["TGID"] = ro.key;
                                        oppRecord["OGID"] = ro.key;

                                        // if both teams are marked as "1" then this is a neutral site game, don't do anything
                                        if ((teamScheduleRecord["THOA"] == "1" && oppRecord["THOA"] == "1") == false)
                                        {
                                            string teamHomeGame = null;
                                            string oppHomeGame = null;
                                            if (f.Abbreviation == "GHTG")
                                            {
                                                teamHomeGame = "1";
                                                oppHomeGame = "0";
                                                mr["SGID"] = teamStadium;
                                            }
                                            else if (f.Abbreviation == "GATG")
                                            {
                                                teamHomeGame = "0";
                                                oppHomeGame = "1";
                                            }

                                            teamScheduleRecord["THOA"] = teamHomeGame;
                                            oppRecord["THOA"] = oppHomeGame;
                                        }
                                    }
                                    else if (mr.Table.Abbreviation == "RCPR")
                                    {
                                        for (int i = 1; i <= 10; i++)
                                        {
                                            var key = i == 10 ? "PT10" : "PT0" + i.ToString();
                                            if (key != f.Abbreviation && mr[key] == ro.key)
                                            {
                                                mr[key] = previousValue;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {	// set by using the index
                                if (((ComboBox)f.EditControl).SelectedIndex > -1)
                                    mr[f.Abbreviation] = ((ComboBox)f.EditControl).SelectedIndex.ToString();
                            }

                            break;

                        case "AdjustedComboBox":
                            if (((ComboBox)f.EditControl).SelectedIndex > -1)
                                mr[f.Abbreviation] = (((ComboBox)f.EditControl).SelectedIndex - f.Offset).ToString();
                            break;

                        case "MappedComboBox":
                            Field f2 = (getMappedField != null) ? getMappedField(f.ControlIF) : null;
                            if (f2 == null)
                            {
                                MessageBox.Show("Could not find mapped field therefore cannot save value!");
                                break;
                            }
                            if (((ComboBox)f.EditControl).SelectedIndex > -1)
                                mr[f2.Abbreviation] = (((ComboBox)f.EditControl).SelectedIndex - f.Offset).ToString();
                            break;

                        case "TimeOfDayInMinutes":
                            string[] stime = f.EditControl.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (stime.Length != 2)
                            {
                                MessageBox.Show("Time format incorrect! Try hh:mm AM/PM");
                                break;
                            }

                            TimeSpan span;
                            if (TimeSpan.TryParse(stime[0], out span))
                            {
                                if (stime[1].ToUpper() == "PM" && span.Hours < 12)
                                    span = span.Add(new TimeSpan(12, 0, 0));
                                mr[f.Abbreviation] = span.TotalMinutes.ToString();
                            }
                            else
                                MessageBox.Show("Time format incorrect! Try hh:mm AM/PM");

                            break;

                        case "TextBox":
                        default:
                            var currentValue = mr[f.Abbreviation] = f.EditControl.Text;

                            if (mr.Table.Abbreviation == "RCPR")
                            {
                                var preValue = mr["PT01"];
                                mr["PT01"] = currentValue;
                                for (int i = 2; i <= 10; i++)
                                {
                                    var key = i == 10 ? "PT10" : "PT0" + i.ToString();
                                    if (key != f.Abbreviation && mr[key] == currentValue)
                                    {
                                        mr[key] = preValue;
                                    }
                                }
                            }

                            break;
                    }

					// perform recalcs
					for( int i=0; i < lChildFields.Count; i++ )
					{	if( lChildFields[ i ].ControlType == "Calculated" )
						{	ColumnHeader	ch	= ((ListView) DisplayControl).Columns[ i +1 ];

							if( ch != null )
							{	args.Item.SubItems[ ch.Index ].Text	= lChildFields[ i ].RunFormula( lChildFields, mr ).ToString( );
							}
						}
					}

				}
			}
		}
		public void GridColumnClicked( object sender, ColumnClickEventArgs e )
		{
			Field	cf	= (Field) ((ListView) DisplayControl).Columns[ e.Column ].Tag;

            var comparer = new ListViewItemComparer(e.Column, cf);
            comparer.isRecruitPitchTable = this.isRecruitPitchTable;
            ((ListView)DisplayControl).ListViewItemSorter = comparer;
			((ListView) DisplayControl).Sort( );

			ListViewItemComparer.sortDir	= -ListViewItemComparer.sortDir;
		}
		public void TabSelecting(object sender, TabControlCancelEventArgs e)
		{
 			if( viewChanged != null && e.TabPage != null )
				viewChanged( (View) e.TabPage.Tag );
		}

		static public View FindView( List<View> lMappedViews, string Name )
		{	foreach( View v in lMappedViews )
			{	if( v.Name == Name )
					return v;
			}
			return null;
		}
		static public bool ProcessAllViewSettings( List<View> lMappedViews, List<Field> lMappedFields )
		{	foreach( View v in lMappedViews )
			{	if( ! v.ProcessSettings( lMappedFields ) )
					return false;
			}
			return true;
		}
		static public bool SetViewChildren( List<View> lMappedViews, Form MainForm )
		{
			// set all to be children of the main form first by default
			foreach( View v in lMappedViews )
				MainForm.Controls.Add( v.DisplayControl );

			foreach( View v in lMappedViews )
			{
				foreach( string s in v.ChildViews )
				{
					View	temp	= View.FindView( lMappedViews, s );
					if( temp != null )
					{	// first remove from the main form
						MainForm.Controls.Remove( temp.DisplayControl );

						// add to child list and control's children
						v.lChildren.Add( temp );

						if( v.Type != "Tab" )
						{
							v.DisplayControl.Controls.Add( temp.DisplayControl );
						}
						else
						{
							TabPage	page	= new TabPage( temp.Name );
							page.Controls.Add( temp.DisplayControl );
							page.Tag		= temp;
							((TabControl) v.DisplayControl).Controls.Add( page );
						}
					}	else
					{
						MessageBox.Show( "Child view " + s + " not found for view " + v.Name, "Error in config" );
						return false;
					}
				}
			}

			TabControl	tab	= null;
			foreach( Control c in MainForm.Controls )
			{	if( c.GetType( ).Name == "TabControl" )
				{	tab	= (TabControl) c;
					break;
				}
			}
			tab.SelectedIndex	= -1;
			tab.SelectedIndex	= 0;

			return true;
		}
	}
    class ListViewItemComparer : IComparer
    {
        private int col;
        public static int sortDir = 1;
        private Field field = null;
        public bool isRecruitPitchTable = false;


        public ListViewItemComparer(int column, Field f)
        {
            col = column;
            field = f;
        }
        public int Compare(object x, object y)
        {
            if (col == 0 || (col == 1 && isRecruitPitchTable))
                return sortDir * Convert.ToInt32(((ListViewItem)x).SubItems[col].Text).CompareTo(Convert.ToInt32(((ListViewItem)y).SubItems[col].Text));

            //if( field.ControlType == "AdjustedComboBox" || field.ControlType == "TimeOfDayInMinutes" )
            //    return sortDir * field.CompareDataAsType( ((MaddenRecord) ((ListViewItem)x).Tag)[ field.name ], ((MaddenRecord) ((ListViewItem)y).Tag)[ field.name ] );

            //if( field.ControlLink == "" )
            //    return sortDir * field.CompareDataAsType( ((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text );

            return sortDir * field.CompareDataAsType(((MaddenRecord)((ListViewItem)x).Tag)[field.name], ((MaddenRecord)((ListViewItem)y).Tag)[field.name]);
        }
    }

    public class ScheduleComparer : IComparer
    {
        public static IComparer Instance = new ScheduleComparer();

        public int Compare(object x, object y)
        {
            var mrx = ((ListViewItem)x).Tag as MaddenRecord;
            var mry = ((ListViewItem)y).Tag as MaddenRecord;

            if (mrx["SEWN"].ToInt32() < mry["SEWN"].ToInt32())
                return -1;
            else if (mrx["SEWN"].ToInt32() == mry["SEWN"].ToInt32())
            {
                return mrx["GHTG"].ToInt32() < mry["GHTG"].ToInt32() ? -1 : 1;
            }
            else
                return 1;
        }
    }

	public class RefObj
	{
		public string	key		= "";
		public string	value	= "";

		public RefObj( )
		{
		}
		public RefObj( string k, string v )
		{	key		= k;
			value	= v;
		}
		public override string ToString()
		{
			return value;
		}
	}
	public class FieldFilter
	{
		public enum Operation
		{
			None,
			Equal,
			NotEqual,
			GreaterThan,
			LessThan,
			Contains,
			DoesNotContain,
			EndsWith,
			StartsWith,
			Set,
			Adjust
		}

		public string		field	= "";
		public string		value	= "";
		public Operation	op		= Operation.None;

		public FieldFilter( )
		{
		}
		public FieldFilter( string f, string operation, string v )
		{	Create( f, operation, v );
		}
		public void Create( string f, string operation, string v )
		{
			field	= f;
			value	= v;

			switch( operation.ToLower( ) )
			{
				case "=":			op	= Operation.Equal;
					break;
				case "!=":			op	= Operation.NotEqual;
					break;
				case ">":			op	= Operation.GreaterThan;
					break;
				case "<":			op	= Operation.LessThan;
					break;
				case "contains":	op	= Operation.Contains;
					break;
				case "!contains":	op	= Operation.DoesNotContain;
					break;
				case "endswith":	op	= Operation.EndsWith;
					break;
				case "startswith":	op	= Operation.StartsWith;
					break;
				// mass operations
				case "<-":			op	= Operation.Set;
					break;
				case "+/-":			op	= Operation.Adjust;
					break;
			}
		}
		public bool Process( List<Field> lMF, MaddenRecord mr )
		{
			Field	f		= Field.FindField( lMF, field );
			string	code	= f.Abbreviation != "" ? f.Abbreviation : f.name;

			switch( op )
			{
				case Operation.Contains:
				case Operation.DoesNotContain:
				case Operation.EndsWith:
				case Operation.StartsWith:

#region is a combox item
					if( f.ControlType.EndsWith( "ComboBox" ) )
					{	string	testData	= "";

						// need to find item based on type of combobox
#region regular combobox with an item list
						if( f.ControlType == "ComboBox" && f.ControlLink == "" )
						{	testData	= ((ComboBox) f.EditControl ).Items[ Convert.ToInt32( mr[ code ] ) ].ToString( );
						}
#endregion
#region linked combobox
						if( f.ControlType == "ComboBox" && f.ControlLink != "" )
						{
							RefObj	linkedObj	= null;
#region find the ojbect in the list
							for( int x=0; x < ((ComboBox) f.EditControl).Items.Count; x++ )
							{
								RefObj	ro	= (RefObj) ((ComboBox) f.EditControl).Items[ x ];
								if( ro.key == mr[ f.Abbreviation ] )
								{	linkedObj	= ro;
									break;
								}
							}
#endregion
							testData	= ( linkedObj != null ) ? linkedObj.value : "";
						}
#endregion
#region adjusted combobox with an item list
						if( f.ControlType == "AdjustedComboBox" )
						{	testData	= ((ComboBox) f.EditControl ).Items[ Convert.ToInt32( mr[ code ] ) + f.Offset ].ToString( );
						}
#endregion
#region mapped combobox with an item list
						if( f.ControlType == "MappedComboBox" )
							testData	= ((ComboBox) f.EditControl ).Items[ Convert.ToInt32( mr[ f.ControlIF ] ) ].ToString( );
#endregion

						switch( op )
						{
							case Operation.Contains:
								return ( testData.IndexOf( value ) == -1 ) ? false : true;

							case Operation.DoesNotContain:
								return ( testData.IndexOf( value ) > -1 ) ? false : true;

							case Operation.EndsWith:
								return testData.EndsWith( value );

							case Operation.StartsWith:
								return testData.StartsWith( value );
						}
					}
#endregion

#region not a combobox variant
					switch( op )
					{
						case Operation.Contains:
							return ( mr[ code ].IndexOf( value ) == -1 ) ? false : true;

						case Operation.DoesNotContain:
							return ( mr[ code ].IndexOf( value ) > -1 ) ? false : true;

						case Operation.EndsWith:
							return mr[ code ].EndsWith( value );

						case Operation.StartsWith:
							return mr[ code ].StartsWith( value );
					}
#endregion
					return false;	// should never get here


				case Operation.Equal:
					return ( f.CompareDataAsType( mr[ code ], value ) == 0 ) ? true : false;

				case Operation.NotEqual:
					return ( f.CompareDataAsType( mr[ code ], value ) != 0 ) ? true : false;

				case Operation.GreaterThan:
					return ( f.CompareDataAsType( mr[ code ], value ) == 1 ) ? true : false;

				case Operation.LessThan:
					return ( f.CompareDataAsType( mr[ code ], value ) == -1 ) ? true : false;

				// mass operations
				case Operation.Set:
					mr[ code ]	= value.ToString( );
					return true;

				case Operation.Adjust:
					mr[ code ]	= ( Convert.ToInt32( mr[ code ] ) + Convert.ToInt32( value.ToString( ) ) ).ToString( );
					return true;
			}
			return false;
		}

		static public bool ProcessFilters( List<FieldFilter> lFF, List<Field> lMF, MaddenRecord mr )
		{	bool	ret	= true;

			if( lFF != null && lMF != null && mr != null )
			{
				foreach( FieldFilter ff in lFF )
					ret	&= ff.Process( lMF, mr );
			}

			return ret;
		}
	}

	// xml config info used in copying known fields / tables
	public class XMLConfig
	{
#region fields
		public string		StartLabel					= "";
		public string		Name						= "";
		public string		Abbreviation				= "";
		public string		Type						= "";
		public string		Description					= "";
		public string		ControlType					= "";
		public bool			ControlLink					= false;
		public string		LinkTable					= "";
		public string		IndexField					= "";
		public string		ReferenceField				= "";
		public string		ReferenceField2				= "";
		public List<string>	ControlItems				= new List<string>( );
		public string		PosX						= "";
		public string		PosY						= "";
		public string		PosZ						= "";
		public string		SizeW						= "";
		public string		SizeH						= "";
		public string		SrcType						= "";
		public string		SrcName						= "";
		public List<string>	Children					= new List<string>( );
		public List<string>	ChildFields					= new List<string>( );
		public string		Min							= "";
		public string		Max							= "";
		public List<Field.Formula>	Formulas			= new List<Field.Formula>( );
		public string		Offset						= "";
#endregion

		public XMLConfig( ){}
		public override string ToString()
		{	string	data	= "";

#region view
			if( StartLabel == "View" )
			{
				data	+= "<View>\r\n";
				data	+= "\t<Name>" + Name + "</Name>\r\n";
				data	+= "\t<Type>" + Type + "</Type>\r\n";
				data	+= "\t<Position>\r\n";
				data	+= "\t\t<X>" + PosX + "</X>\r\n";
				data	+= "\t\t<Y>" + PosY + "</Y>\r\n";
				data	+= "\t\t<Z>" + PosZ + "</Z>\r\n";
				data	+= "\t</Position>\r\n";
				data	+= "\t<Size>\r\n";
				data	+= "\t\t<Width>"  + SizeW + "</Width>\r\n";
				data	+= "\t\t<Height>" + SizeH + "</Height>\r\n";
				data	+= "\t</Size>\r\n";

				if( SrcName != "" && SrcType != "" )
				{
					data	+= "\t<Source>\r\n";
					data	+= "\t\t<Type>" + SrcType + "</Type>\r\n";
					data	+= "\t\t<Name>" + SrcName + "</Name>\r\n";
					data	+= "\t</Source>\r\n";
				}

				if( Children.Count >0 )
				{
					foreach( string s in Children )
						data	+= "\t<Child>" + s + "</Child>\r\n";
				}

				if( ChildFields.Count >0 )
				{
					foreach( string s in ChildFields )
						data	+= "\t<Field>" + s + "</Field>\r\n";
				}

				data	+= "</View>\r\n\r\n\r\n";
			}
#endregion
#region table
			if( StartLabel == "Table" )
			{
				data	+= "<Table>\r\n";
				data	+= "\t<Abbreviation>" + Abbreviation + "</Abbreviation>\r\n";
				data	+= "\t<Name>" + Name + "</Name>\r\n";
				data	+= "</Table>\r\n\r\n";
			}
#endregion
#region field
			if( StartLabel == "Field" )
			{
				data	+= "<Field>\r\n";
				data	+= "\t<Abbreviation>" + Abbreviation + "</Abbreviation>\r\n";
				data	+= "\t<Name>" + Name + "</Name>\r\n";
				data	+= "\t<ControlType>" + ControlType + "</ControlType>\r\n";

				if( ControlItems.Count >0 )
				{
					foreach( string s in ControlItems )
						data	+= "\t<ControlItem>" + s + "</ControlItem>\r\n";
				}

				if( ControlLink )
				{
					data	+= "\t<ControlLink>\r\n";
					data	+= "\t\t<Table>" + LinkTable + "</Table>\r\n";
					data	+= "\t\t<IndexField>" + IndexField + "</IndexField>\r\n";

					if( ReferenceField != "" )
					data	+= "\t\t<ReferenceField>" + ReferenceField + "</ReferenceField>\r\n";
					if( ReferenceField2 != "" )
						data	+= "\t\t<ReferenceField2>" + ReferenceField2 + "</ReferenceField2>\r\n";

					if( Formulas.Count >0 )
					{
						data	+= "\t\t<Formulas>\r\n";
						foreach( Field.Formula form in Formulas )
						{
							data	+= "\t\t\t<Formula>\r\n";
							data	+= "\t\t\t\t<IndexValue>" + form.IndexValue + "</IndexValue>\r\n";
							if( form.Variables.Count >0 )
							{
								data	+= "\t\t\t\t<Variables>\r\n";
								foreach( Field.Variable var in form.Variables )
								{
									data	+= "\t\t\t\t\t<Variable>\r\n";
									data	+= "\t\t\t\t\t\t<Field>" + var.vField + "</Field>\r\n";
									data	+= "\t\t\t\t\t\t<Multiplier>" + var.Multiplier.ToString( ) + "</Multiplier>\r\n";
									data	+= "\t\t\t\t\t</Variable>\r\n";
								}
								data	+= "\t\t\t\t</Variables>\r\n";
							}
							data	+= "\t\t\t\t<Adjustment>" + form.Adjustment.ToString( ) + "</Adjustment>\r\n";
							data	+= "\t\t\t</Formula>\r\n";
						}
						data	+= "\t\t</Formulas>\r\n";
					}

					if( Min != "" )
						data	+= "\t\t<Min>" + Min + "</Min>\r\n";
					if( Max != "" )
						data	+= "\t\t<Max>" + Max + "</Max>\r\n";
					data	+= "\t</ControlLink>\r\n";
				}

				if( Offset != "" )
				data	+= "\t<Offset>" + Offset + "</Offset>\r\n";
				data	+= "\t<Description>" + Description + "</Description>\r\n";
				data	+= "\t<Type>" + Type + "</Type>\r\n";
				data	+= "</Field>\r\n";
			}
#endregion

			return data;
		}
		public void Copy( XMLConfig org )
		{
			StartLabel					= org.StartLabel;
			Name						= org.Name;
			Abbreviation				= org.Abbreviation;
			Type						= org.Type;
			Description					= org.Description;
			ControlType					= org.ControlType;
			ControlLink					= org.ControlLink;
			LinkTable					= org.LinkTable;
			IndexField					= org.IndexField;
			ReferenceField				= org.ReferenceField;
			ReferenceField2				= org.ReferenceField2;
			ControlItems				= new List<string>( org.ControlItems );
			PosX						= org.PosX;
			PosY						= org.PosY;
			PosZ						= org.PosZ;
			SizeW						= org.SizeW;
			SizeH						= org.SizeH;
			SrcType						= org.SrcType;
			SrcName						= org.SrcName;
			Children					= new List<string>( org.Children );
			ChildFields					= new List<string>( org.ChildFields );
			Min							= org.Min;
			Max							= org.Max;
			Formulas					= new List<Field.Formula>( org.Formulas );
		}

		static public void ReadXMLConfig( string configfile, List<XMLConfig> views, List<XMLConfig> tables, List<XMLConfig> fields )
		{
			XMLConfig			xml				= null;
			string				Path			= "\\";


			XmlTextReader	reader	= new XmlTextReader( configfile );
			while( reader.Read( ) )
			{
				switch( reader.NodeType )
				{
					case XmlNodeType.Element:
#region map open elements
						if( Path == "\\xml\\" && reader.Name == "Field" )
						{	xml				= new XMLConfig( );
							xml.StartLabel	= "Field";
						}

						if( Path == "\\xml\\" && reader.Name == "Table" )
						{	xml				= new XMLConfig( );
							xml.StartLabel	= "Table";
						}

						if( Path == "\\xml\\" && (reader.Name == "View" || reader.Name == "Main") )
						{	xml				= new XMLConfig( );
							xml.StartLabel	= "View";
						}

						if( reader.Name == "Formulas" )
						{	xml.Formulas	= Field.Formula.ReadFormulas( reader, Path + "Formulas\\" );
							break;
						}
#endregion

						Path	+= reader.Name + "\\";
						break;

					case XmlNodeType.Text:

#region map main entries	
						if( Path.EndsWith( "Main\\Size\\Width\\" ) )
							xml.SizeW	= reader.Value;

						if( Path.EndsWith( "Main\\Size\\Height\\" ) )
							xml.SizeH	= reader.Value;
#endregion

#region map field entries
						if( Path.EndsWith( "Field\\Abbreviation\\" ) )
							xml.Abbreviation	= reader.Value;

						if( Path.EndsWith( "Field\\Name\\" ) )
							xml.Name			= reader.Value;

						if( Path.EndsWith( "Field\\ControlType\\" ) )
							xml.ControlType		= reader.Value;

						if( Path.EndsWith( "Field\\ControlItem\\" ) )
							xml.ControlItems.Add( reader.Value );

						if( Path.EndsWith( "Field\\ControlLink\\Table\\" ) )
						{	xml.ControlLink		= true;
							xml.LinkTable		= reader.Value;
						}

						if( Path.EndsWith( "Field\\ControlLink\\IndexField\\" ) )
						{	xml.ControlLink		= true;
							xml.IndexField		= reader.Value;
						}

						if( Path.EndsWith( "Field\\ControlLink\\ReferenceField\\" ) )
						{	xml.ControlLink		= true;
							xml.ReferenceField	= reader.Value;
						}

						if( Path.EndsWith( "Field\\ControlLink\\ReferenceField2\\" ) )
						{	xml.ControlLink		= true;
							xml.ReferenceField2	= reader.Value;
						}

						if( Path.EndsWith( "Field\\ControlLink\\Formulas\\" ) )
						{	xml.ControlLink		= true;
							xml.Formulas		= Field.Formula.ReadFormulas( reader, Path );
						}

						if( Path.EndsWith( "Field\\ControlLink\\Min\\" ) )
						{	xml.ControlLink		= true;
							xml.Min				= reader.Value;
						}

						if( Path.EndsWith( "Field\\ControlLink\\Max\\" ) )
						{	xml.ControlLink		= true;
							xml.Max				= reader.Value;
						}

						if( Path.EndsWith( "Field\\Offset\\" ) )
							xml.Offset			= reader.Value;

						if( Path.EndsWith( "Field\\Description\\" ) )
							xml.Description		= reader.Value;

						if( Path.EndsWith( "Field\\Type\\" ) )
							xml.Type			= reader.Value;
#endregion

#region map table entries	
						if( Path.EndsWith( "Table\\Abbreviation\\" ) )
							xml.Abbreviation	= reader.Value;

						if( Path.EndsWith( "Table\\Name\\" ) )
							xml.Name			= reader.Value;
#endregion

#region map view entries
						if( Path.EndsWith( "View\\Name\\" ) )
							xml.Name			= reader.Value;

						if( Path.EndsWith( "View\\Type\\" ) )
							xml.Type			= reader.Value;

						if( Path.EndsWith( "View\\Source\\Type\\" ) )
							xml.SrcType			= reader.Value;

						if( Path.EndsWith( "View\\Source\\Name\\" ) )
							xml.SrcName			= reader.Value;

						if( Path.EndsWith( "View\\Position\\X\\" ) )
							xml.PosX			= reader.Value;

						if( Path.EndsWith( "View\\Position\\Y\\" ) )
							xml.PosY			= reader.Value;

						if( Path.EndsWith( "View\\Position\\Z\\" ) )
							xml.PosZ			= reader.Value;

						if( Path.EndsWith( "View\\Size\\Width\\" ) )
							xml.SizeW			= reader.Value;

						if( Path.EndsWith( "View\\Size\\Height\\" ) )
							xml.SizeH			= reader.Value;

						if( Path.EndsWith( "View\\Child\\" ) )
							xml.Children.Add( reader.Value );

						if( Path.EndsWith( "View\\Field\\" ) )
							xml.ChildFields.Add( reader.Value );
#endregion
						break;

					case XmlNodeType.EndElement:
#region map close elements
						if( Path == "\\xml\\Field\\" && reader.Name == "Field" )
							fields.Add( xml );

						if( Path == "\\xml\\Table\\" && reader.Name == "Table" )
							tables.Add( xml );

						if( Path == "\\xml\\View\\" && reader.Name == "View" )
							views.Add( xml );
#endregion

						try
						{	Path	= Path.Remove( Path.LastIndexOf( reader.Name + "\\" ) );
						}	catch( Exception e )
						{
							MessageBox.Show( "XML closing element not found: " + reader.Name + ", " + reader.LineNumber, "Error in XML config" );
							throw( e );
						}
						break;
				}
			}
			reader.Close( );
			return;
		}
		static public void CopyMappedValues( List<XMLConfig> from, List<XMLConfig> to )
		{
			foreach( XMLConfig xml in to )
			{
				XMLConfig	f	= from.Find( (x) => x.Abbreviation == xml.Abbreviation );
				if( f != null )
				{
					if( f.Name != "" )
						xml.Copy( f );
				}

			}
		}
		static public void UseFriendlyNames( List<XMLConfig> views, List<XMLConfig> tables, List<XMLConfig> fields )
		{
			foreach( XMLConfig v in views )
			{
				// make sure source table is using the friendly name
				XMLConfig	t	= tables.Find( (a) => a.Abbreviation == v.SrcName );
				if( t != null && t.Name != "" && t.Name != t.Abbreviation )
					v.SrcName	= t.Name;

				// now do the same for the fields
				for( int i=0; i < v.ChildFields.Count; i++ )
				{
					XMLConfig	f	= fields.Find( (a) => a.Abbreviation == v.ChildFields[ i ] );
					if( f != null && f.Name != "" && f.Name != f.Abbreviation )
						v.ChildFields[ i ]	= f.Name;
				}
			}

		}
		static public string WriteXMLConfig( List<XMLConfig> views, List<XMLConfig> tables, List<XMLConfig> fields )
		{	string	data	= "<xml>\r\n\r\n\r\n";

			foreach( XMLConfig x in views )
				data	+= x.ToString( );
			foreach( XMLConfig x in tables )
				data	+= x.ToString( );
			foreach( XMLConfig x in fields )
				data	+= x.ToString( );

			return data + "\r\n\r\n</xml>\r\n";
		}
	}
}
