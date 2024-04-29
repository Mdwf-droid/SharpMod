using System.Windows.Forms;
namespace SharpMod.Win.UI
{
    partial class VuMeter
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private PictureBox pbMeter;
        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pbMeter = new System.Windows.Forms.PictureBox();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbMeter)).BeginInit();
            this.SuspendLayout();
            // 
            // pbMeter
            // 
            this.pbMeter.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.pbMeter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbMeter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbMeter.Location = new System.Drawing.Point(3, 3);
            this.pbMeter.Name = "pbMeter";
            this.pbMeter.Size = new System.Drawing.Size(224, 128);
            this.pbMeter.TabIndex = 0;
            this.pbMeter.TabStop = false;
            // 
            // timerRefresh
            // 
            this.timerRefresh.Tick += new System.EventHandler(this.TimerRefresh_Tick);
            // 
            // VuMeter
            // 
            this.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.Controls.Add(this.pbMeter);
            this.Name = "VuMeter";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(230, 134);
            ((System.ComponentModel.ISupportInitialize)(this.pbMeter)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Timer timerRefresh;
    }
}
