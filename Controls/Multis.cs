/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class Multis : UserControl
    {
        public Multis()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refmarker = this;
        }

        private bool Loaded = false;
        private bool ShowFreeSlots = false;
        private Multis refmarker;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
        }
        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Multis"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            TreeViewMulti.BeginUpdate();
            TreeViewMulti.Nodes.Clear();
            for (int i = 0; i < 0x2000; i++)
            {
                MultiComponentList multi = Ultima.Multis.GetComponents(i);
                if (multi != MultiComponentList.Empty)
                {
                    TreeNode node = new TreeNode(String.Format("{0,5} (0x{1:X})", i, i));
                    node.Tag = multi;
                    node.Name = i.ToString();
                    TreeViewMulti.Nodes.Add(node);
                }

            }
            TreeViewMulti.EndUpdate();
            if (TreeViewMulti.Nodes.Count > 0)
                TreeViewMulti.SelectedNode = TreeViewMulti.Nodes[0];
            if (!Loaded)
                FiddlerControls.Options.FilePathChangeEvent += new FiddlerControls.Options.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        public void ChangeMulti(int id, MultiComponentList multi)
        {
            if (multi != MultiComponentList.Empty)
            {
                int index = refmarker.TreeViewMulti.SelectedNode.Index;
                if (int.Parse(refmarker.TreeViewMulti.SelectedNode.Name) != id)
                {
                    for (int i = 0; i < refmarker.TreeViewMulti.Nodes.Count; i++)
                    {
                        if (int.Parse(refmarker.TreeViewMulti.Nodes[i].Name) == id)
                        {
                            index = i;
                            break;
                        }
                    }
                }
                refmarker.TreeViewMulti.Nodes[index].Tag = multi;
                refmarker.TreeViewMulti.Nodes[index].ForeColor = Color.Black;
                if (index != refmarker.TreeViewMulti.SelectedNode.Index)
                    refmarker.TreeViewMulti.SelectedNode = refmarker.TreeViewMulti.Nodes[index];

                afterSelect_Multi(this, null);
            }
        }

        private void afterSelect_Multi(object sender, TreeViewEventArgs e)
        {
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                HeightChangeMulti.Maximum = 0;
                toolTip.SetToolTip(HeightChangeMulti, "MaxHeight: 0");
                StatusMultiText.Text = "Size: 0,0 MaxHeight: 0 MultiRegion: 0,0,0,0";
            }
            else
            {
                HeightChangeMulti.Maximum = multi.maxHeight;
                toolTip.SetToolTip(HeightChangeMulti, String.Format("MaxHeight: {0}", HeightChangeMulti.Maximum - HeightChangeMulti.Value));
                StatusMultiText.Text = String.Format("Size: {0},{1} MaxHeight: {2} MultiRegion: {3},{4},{5},{6} Surface: {7}",
                                                   multi.Width,
                                                   multi.Height,
                                                   multi.maxHeight,
                                                   multi.Min.X,
                                                   multi.Min.Y,
                                                   multi.Max.X,
                                                   multi.Max.Y,
                                                   multi.Surface);
            }
            ChangeComponentList(multi);
            MultiPictureBox.Refresh();
        }

        private void onPaint_MultiPic(object sender, PaintEventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
                return;
            if ((MultiComponentList)TreeViewMulti.SelectedNode.Tag == MultiComponentList.Empty)
            {
                e.Graphics.Clear(Color.White);
                return;
            }
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap m_MainPicture_Multi = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            if (m_MainPicture_Multi == null)
            {
                e.Graphics.Clear(Color.White);
                return;
            }
            Point location = Point.Empty;
            Size size = MultiPictureBox.Size;
            Rectangle destRect = Rectangle.Empty;
            if ((m_MainPicture_Multi.Height < size.Height) && (m_MainPicture_Multi.Width < size.Width))
            {
                location.X = (MultiPictureBox.Width - m_MainPicture_Multi.Width) / 2;
                location.Y = (MultiPictureBox.Height - m_MainPicture_Multi.Height) / 2;
                destRect = new Rectangle(location, m_MainPicture_Multi.Size);
            }
            else if (m_MainPicture_Multi.Height < size.Height)
            {
                location.X = 0;
                location.Y = (MultiPictureBox.Height - m_MainPicture_Multi.Height) / 2;
                if (m_MainPicture_Multi.Width > size.Width)
                    destRect = new Rectangle(location, new Size(size.Width, m_MainPicture_Multi.Height));
                else
                    destRect = new Rectangle(location, m_MainPicture_Multi.Size);
            }
            else if (m_MainPicture_Multi.Width < size.Width)
            {
                location.X = (MultiPictureBox.Width - m_MainPicture_Multi.Width) / 2;
                location.Y = 0;
                if (m_MainPicture_Multi.Height > size.Height)
                    destRect = new Rectangle(location, new Size(m_MainPicture_Multi.Width, size.Height));
                else
                    destRect = new Rectangle(location, m_MainPicture_Multi.Size);
            }
            else
                destRect = new Rectangle(new Point(0, 0), size);


            e.Graphics.DrawImage(m_MainPicture_Multi, destRect, 0, 0, m_MainPicture_Multi.Width, m_MainPicture_Multi.Height, System.Drawing.GraphicsUnit.Pixel);

        }

        private void onValue_HeightChangeMulti(object sender, EventArgs e)
        {
            toolTip.SetToolTip(HeightChangeMulti, String.Format("MaxHeight: {0}", HeightChangeMulti.Maximum - HeightChangeMulti.Value));
            MultiPictureBox.Refresh();
        }

        private void ChangeComponentList(MultiComponentList multi)
        {
            MultiComponentBox.Clear();
            if (multi != MultiComponentList.Empty)
            {
                for (int x = 0; x < multi.Width; ++x)
                {
                    for (int y = 0; y < multi.Height; ++y)
                    {
                        Tile[] tiles = multi.Tiles[x][y];
                        for (int i = 0; i < tiles.Length; ++i)
                        {
                            MultiComponentBox.AppendText(String.Format("0x{0:X4} {1,3} {2,3} {3,2}\n", tiles[i].ID - 0x4000, x, y, tiles[i].Z));
                        }
                    }
                }
            }
        }

        private void extract_Image_ClickBmp(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Multi 0x{0:X}.bmp", int.Parse(TreeViewMulti.SelectedNode.Name)));
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap bit = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            bit.Save(FileName, ImageFormat.Bmp);
            MessageBox.Show(String.Format("Multi saved to {0}", FileName), "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickTiff(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Multi 0x{0:X}.tiff", int.Parse(TreeViewMulti.SelectedNode.Name)));
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap bit = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            bit.Save(FileName, ImageFormat.Tiff);
            MessageBox.Show(String.Format("Multi saved to {0}", FileName), "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickJpg(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Multi 0x{0:X}.jpg", int.Parse(TreeViewMulti.SelectedNode.Name)));
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap bit = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            bit.Save(FileName, ImageFormat.Jpeg);
            MessageBox.Show(String.Format("Multi saved to {0}", FileName), "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickFreeSlots(object sender, EventArgs e)
        {
            ShowFreeSlots = !ShowFreeSlots;
            TreeViewMulti.BeginUpdate();
            TreeViewMulti.Nodes.Clear();
            if (ShowFreeSlots)
            {
                for (int i = 0; i < 0x2000; i++)
                {
                    MultiComponentList multi = Ultima.Multis.GetComponents(i);
                    TreeNode node = new TreeNode(String.Format("{0,5} (0x{1:X})", i, i));
                    node.Name = i.ToString();
                    node.Tag = multi;
                    if (multi == MultiComponentList.Empty)
                        node.ForeColor = Color.Red;
                    TreeViewMulti.Nodes.Add(node);
                }
            }
            else
            {
                for (int i = 0; i < 0x2000; i++)
                {
                    MultiComponentList multi = Ultima.Multis.GetComponents(i);
                    if (multi != MultiComponentList.Empty)
                    {
                        TreeNode node = new TreeNode(String.Format("{0,5} (0x{1:X})", i, i));
                        node.Tag = multi;
                        node.Name = i.ToString();
                        TreeViewMulti.Nodes.Add(node);
                    }
                }
            }
            TreeViewMulti.EndUpdate();
        }

        private void OnExportTextFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
                return;
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
                return;
            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Multi 0x{0:X}.txt", id));
            multi.ExportToTextFile(FileName);
            MessageBox.Show(String.Format("Multi saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnExportWscFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
                return;
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
                return;
            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Multi 0x{0:X}.wsc", id));
            multi.ExportToWscFile(FileName);
            MessageBox.Show(String.Format("Multi saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnExportUOAFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
                return;
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
                return;
            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName = Path.Combine(path, String.Format("Multi 0x{0:X}.uoa", id));
            multi.ExportToUOAFile(FileName);
            MessageBox.Show(String.Format("Multi saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }


        private void OnClickSave(object sender, EventArgs e)
        {
            Ultima.Multis.Save(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            MessageBox.Show(
                    String.Format("Saved to {0}", AppDomain.CurrentDomain.SetupInformation.ApplicationBase),
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Multis"] = false;
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
                return;
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
                return;
            int id = int.Parse(TreeViewMulti.SelectedNode.Name);
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove {0} (0x{0:X})", id),
                        "Remove",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Ultima.Multis.Remove(id);
                TreeViewMulti.SelectedNode.Remove();
                Options.ChangedUltimaClass["Multis"] = true;
            }
        }

        FiddlerControls.MultiImport multiimport = null;
        private void OnClickImport(object sender, EventArgs e)
        {
            if ((multiimport == null) || (multiimport.IsDisposed))
            {
                MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
                int id = int.Parse(TreeViewMulti.SelectedNode.Name);
                if (multi != MultiComponentList.Empty)
                {
                    DialogResult result =
                            MessageBox.Show(String.Format("Are you sure to replace {0} (0x{0:X})", id),
                            "Import",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                        return;
                }

                multiimport = new MultiImport(this, id);
                multiimport.TopMost = true;
                multiimport.Show();
            }
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < refmarker.TreeViewMulti.Nodes.Count; i++)
                    {
                        int index = (int)refmarker.TreeViewMulti.Nodes[i].Index;
                        if (index >= 0)
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Multi 0x{0:X}.bmp", index));
                            int h = 120;
                            Bitmap bit = ((MultiComponentList)refmarker.TreeViewMulti.Nodes[i].Tag).GetImage(h);
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Bmp);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All Multis saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < refmarker.TreeViewMulti.Nodes.Count; i++)
                    {
                        int index = (int)refmarker.TreeViewMulti.Nodes[i].Index;
                        if (index >= 0)
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Multi 0x{0:X}.tiff", index));
                            int h = 120;
                            Bitmap bit = ((MultiComponentList)refmarker.TreeViewMulti.Nodes[i].Tag).GetImage(h);
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Tiff);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All Multis saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < refmarker.TreeViewMulti.Nodes.Count; i++)
                    {
                        int index = (int)refmarker.TreeViewMulti.Nodes[i].Index;
                        if (index >= 0)
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Multi 0x{0:X}.jpg", index));
                            int h = 120;
                            Bitmap bit = ((MultiComponentList)refmarker.TreeViewMulti.Nodes[i].Tag).GetImage(h);
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Jpeg);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All Multis saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllText(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < refmarker.TreeViewMulti.Nodes.Count; i++)
                    {
                        int index = (int)refmarker.TreeViewMulti.Nodes[i].Index;
                        if (index >= 0)
                        {
                            MultiComponentList multi = (MultiComponentList)refmarker.TreeViewMulti.Nodes[i].Tag;
                            if (multi == MultiComponentList.Empty)
                                continue;
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Multi 0x{0:X}.txt", index));
                            multi.ExportToTextFile(FileName);
                        }
                    }
                    MessageBox.Show(String.Format("All Multis saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllUOA(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < refmarker.TreeViewMulti.Nodes.Count; i++)
                    {
                        int index = (int)refmarker.TreeViewMulti.Nodes[i].Index;
                        if (index >= 0)
                        {
                            MultiComponentList multi = (MultiComponentList)refmarker.TreeViewMulti.Nodes[i].Tag;
                            if (multi == MultiComponentList.Empty)
                                continue;
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Multi 0x{0:X}.uoa", index));
                            multi.ExportToUOAFile(FileName);
                        }
                    }
                    MessageBox.Show(String.Format("All Multis saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllWSC(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < refmarker.TreeViewMulti.Nodes.Count; i++)
                    {
                        int index = (int)refmarker.TreeViewMulti.Nodes[i].Index;
                        if (index >= 0)
                        {
                            MultiComponentList multi = (MultiComponentList)refmarker.TreeViewMulti.Nodes[i].Tag;
                            if (multi == MultiComponentList.Empty)
                                continue;
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Multi 0x{0:X}.wsc", index));
                            multi.ExportToWscFile(FileName);
                        }
                    }
                    MessageBox.Show(String.Format("All Multis saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }
    }
}
