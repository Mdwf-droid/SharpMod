using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Microsoft.Xna.Framework.Media;
using SharpMod.DSP;
using SharpMod.SoundRenderer;
#if WINDOWS_PHONE
using Microsoft.Devices.Sensors;


#endif
namespace SharpMod.XNA.WP7.Demo_VS2010
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SharpModApp : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        SpriteFont _LabelFont;
        SpriteFont _NormalFont;
        SpriteFont _titlefont;
        Texture2D[] _textures;
        SpriteFont _font;
        //private BitmapImage wb;
        VisualizationData _visualizationData;
        Song.SongModule _sm;
        private ModulePlayer _mp;

        // Visualization logic
        int _centerX;
        int _centerY;
        const int BarWidth = 10;
        const int BarHeight = 500;
        const byte MaxValue = 255;
        const byte Alpha = 200;
        Color _targetColor = new Color(MaxValue, 0, 0, Alpha);
        float _rotation;
        bool _spin;
        bool _cycle;
        int _textureIndex;
        bool _paused;

        ///<summary>
        ///</summary>
        public SharpModApp()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
#if WINDOWS_PHONE
            // Handle hiding of the battery status bar when used on windows phone
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = 480;
            _graphics.PreferredBackBufferHeight = 800;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 30.0);
#else

            /*now handle hires screens for xbox and windows*/

            _graphics.IsFullScreen = false;
            _graphics.PreparingDeviceSettings += GraphicsPreparingDeviceSettings;
             
            // Frame Rate is 60 fps by default for Xbox 360 and Windows.
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 60.0);
#endif

            var dsei = new DynamicSoundEffectInstance(48000, AudioChannels.Stereo);

            var ms = new MemoryStream(ModResources.Babylon);
            _sm = ModuleLoader.Instance.LoadModule(ms);
            _mp = new ModulePlayer(_sm);
            _mp.RegisterRenderer(new XnaSoundRenderer(dsei));
            _mp.DspAudioProcessor = new AudioProcessor(1024, 50);
            _mp.DspAudioProcessor.OnCurrentSampleChanged += DspAudioProcessor_OnCurrentSampleChanged;
            _mp.Start();

        }

        private void DrawText()
        {
            _spriteBatch.DrawString(_titlefont, "Now Playing", new Vector2(20, 45), Color.White);
            const int topPos = 105;
            // Headings
            _spriteBatch.DrawString(_LabelFont, "Module Name", new Vector2(20, topPos), Color.White); // TODO: Add in module title
            _spriteBatch.DrawString(_LabelFont, "Module Type", new Vector2(20, topPos + 40), Color.White);
            _spriteBatch.DrawString(_LabelFont, "No Channels", new Vector2(20, topPos + 80), Color.White);
            _spriteBatch.DrawString(_LabelFont, "Song Position", new Vector2(20, topPos + 120), Color.White);

            // InfoText
            _spriteBatch.DrawString(_NormalFont, ": " + CleanInput(_sm.SongName), new Vector2(150, topPos), Color.White); // TODO: Add in module title
            _spriteBatch.DrawString(_NormalFont, ": " + _sm.ModType.Trim(), new Vector2(150, topPos + 40), Color.White);
            _spriteBatch.DrawString(_NormalFont, ": " + _sm.ChannelsCount, new Vector2(150, topPos + 80), Color.White);
            _spriteBatch.DrawString(_NormalFont, ": " + _mp.PlayerInstance.mp_sngpos + "/" + _mp.CurrentModule.Patterns[_mp.PlayerInstance.mp_sngpos].RowsCount, new Vector2(150, topPos + 120), Color.White);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Clean input. </summary>
        ///
        /// <remarks>   Walter, 15/11/2011. </remarks>
        ///
        /// <param name="strIn">    The string in. </param>
        ///
        /// <returns>   The cleaned string. </returns>
        ///-------------------------------------------------------------------------------------------------

        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            return Regex.Replace(strIn, @"[^\w\.@-]", "");
        }

        void GraphicsPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
#if XBOX
            foreach (var displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                // High def resolution support for xbox (if available)
                if (displayMode.Width == 1920 || displayMode.Width == 1080)
                {
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat = displayMode.Format;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = displayMode.Height;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = displayMode.Width;
                }
            }
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            
            // Load textures
            _textures = new Texture2D[5];
            _textures[0] = Content.Load<Texture2D>("Textures\\square");
            _textures[1] = Content.Load<Texture2D>("Textures\\circle2");
            _textures[2] = Content.Load<Texture2D>("Textures\\circle");
            _textures[3] = Content.Load<Texture2D>("Textures\\zebra");
            _textures[4] = Content.Load<Texture2D>("Textures\\geo");

            // Load fonts
            _font = Content.Load<SpriteFont>("Fonts\\MyFont");
            _LabelFont = Content.Load<SpriteFont>("Fonts\\LabelFont");
            _NormalFont = Content.Load<SpriteFont>("Fonts\\NormalFont");
            _titlefont = Content.Load<SpriteFont>("Fonts\\TitleFont");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here

            //VuMeterLeft.Update();
           // VuMeterRight.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            DrawText();
            // TODO: add in control buttons
            // TODO: add in visualiser
            _spriteBatch.End();
            
            // TODO: Add your drawing code here);

            base.Draw(gameTime);
        }

        void DspAudioProcessor_OnCurrentSampleChanged(int[] leftSample, int[] rightSample)
        {
           // VuMeterLeft.Process(leftSample);
            //VuMeterRight.Process(rightSample);
        }
       
    }
}
