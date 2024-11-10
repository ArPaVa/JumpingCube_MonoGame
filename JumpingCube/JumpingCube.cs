using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Net.Mime;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace JumpingCube;

public class JumpingCube : Game
{   private bool died;
    private bool double_jump;
    private int double_jump_timer;
    private int score;
    private int highscore;
    private int next_difficulty;
    private int next_key;
    private Texture2D cube;
    private Texture2D cubedj;
    private int[] Cube_position {get; set;}
    private int[] Cube_dimensions {get; set;}

    private SoundEffect jumping_sound;
    private Texture2D pixel;

    private Texture2D rectangle;
    private List<BadGuyRectangle> Bg_rect {get; set;}
    private int[] Rectangles_dimensions {get; set;}
     private int speed { get; set;}

    private int x_impulse { get; set; }
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;   
    private SpriteFont scores_font;
    private int next_obstacle {get; set;}
    public JumpingCube()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();  
    }

    protected override void Initialize()
    {   
        highscore = 0;
        score = 0;
        next_difficulty = 0;
        
        died = false;
        double_jump = true;
        double_jump_timer = 0;

        Cube_position =  new int[2];
        Cube_position[0]= 30;
        Cube_position[1]= _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;

        Cube_dimensions = new int[2];
        Cube_dimensions[0]= _graphics.PreferredBackBufferHeight/10;
        Cube_dimensions[1]= _graphics.PreferredBackBufferHeight/10;

        Rectangles_dimensions = new int[2];
        Rectangles_dimensions[0]= _graphics.PreferredBackBufferWidth/16;
        Rectangles_dimensions[1]= _graphics.PreferredBackBufferHeight/6;

        next_key=0;
        x_impulse = 0;
        speed = Cube_dimensions[0]/10;
        next_obstacle = 0;
        Bg_rect = [];
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        cube = Content.Load<Texture2D>("cube");
        cubedj = Content.Load<Texture2D>("cubedj");
        cube = ChangeColorToTransparent(GraphicsDevice,cube,Color.White);
        cubedj = ChangeColorToTransparent(GraphicsDevice,cubedj,Color.White);
        jumping_sound = Content.Load<SoundEffect>("jumping_sound");
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        rectangle = Content.Load<Texture2D>("rectangle");
        scores_font = Content.Load<SpriteFont>("scores_font");
        Color[] data = new Color[1];
        data[0] = Color.White; 
        pixel.SetData(data);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {   if(!died)
        {   next_key-=1;
            next_difficulty +=1;
            if (next_difficulty>=250)
            {next_difficulty = 0;
            speed += Cube_dimensions[0]/70;

            }
            score += 1;
            for (int i = 0; i<Bg_rect.Count;i++)
            {   Rectangle cube_rectangle  = new Rectangle (Cube_position[0],Cube_position[1],Cube_dimensions[0],Cube_dimensions[1]); 
                Rectangle rectangle_rectangle = new Rectangle (Bg_rect[i].x_pos,Bg_rect[i].y_pos-Rectangles_dimensions[1],Rectangles_dimensions[0],Rectangles_dimensions[1]);
                if (cube_rectangle.Intersects(rectangle_rectangle))
                    died = true;
                
            }
            if (!died)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();

                if (Cube_position[1] + x_impulse > _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10)
                    Cube_position[1] = _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;
                else
                    Cube_position[1] += x_impulse;

                if (Cube_position[1] >=  _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 )
                    Cube_position[1] = _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;
                if ((-(Cube_dimensions[0]/10) < x_impulse && x_impulse<0) ||  (x_impulse< Cube_dimensions[0]/10 && x_impulse> 0))
                    x_impulse += Cube_dimensions[0]/60;
                else
                    if (x_impulse <= Cube_dimensions[0]/4)
                        x_impulse +=Cube_dimensions[0]/40;
                    
                if (CubeIsSuported())
                    x_impulse = 0;
                
                if (!CubeIsSuported() && Keyboard.GetState().IsKeyDown(Keys.Space) && double_jump && next_key<0)
                {   double_jump = false;
                    double_jump_timer = 200;
                    x_impulse = (int)-(Cube_dimensions[0]/4);
                    jumping_sound.Play();
                    next_key = 10;
                }
                if (CubeIsSuported() && Keyboard.GetState().IsKeyDown(Keys.Space)&& next_key<0)
                {   x_impulse = (int)-(Cube_dimensions[0]/3.3);
                    jumping_sound.Play();
                    next_key =10;
                }
                
                if (Bg_rect != null)
                for (int i= 0;i < Bg_rect.Count;i ++)
                {
                    Bg_rect[i].x_pos -= speed;
                    if (Bg_rect[i].x_pos<-70)
                        Bg_rect.RemoveAt(i);

                }
                double_jump_timer -= 1;
                if (double_jump_timer<=0 && CubeIsSuported())
                    double_jump = true;
                next_obstacle-= speed;
                if (next_obstacle <= 0)
                {
                    Random random = new Random();
                    int randm = random.Next(1,30);
                    if (randm >= 30-speed)
                    {
                        Bg_rect.Add(new BadGuyRectangle(_graphics.PreferredBackBufferWidth,_graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4,rectangle));
                        int randm_gap = random.Next(1,Rectangles_dimensions[1]/2);
                        next_obstacle = 3*Rectangles_dimensions[1] + randm_gap*speed;
                        
                    }}}}
        else
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {   
                if (score>highscore)
                    highscore = score;
                score = 0;
                next_difficulty = 0;
                
                died = false;
                double_jump=true;
                double_jump_timer = 0;
                Cube_position =  new int[2];
                Cube_position[0]= 30;
                Cube_position[1]= _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;

                Cube_dimensions = new int[2];
                Cube_dimensions[0]= _graphics.PreferredBackBufferHeight/10;
                Cube_dimensions[1]= _graphics.PreferredBackBufferHeight/10;

                Rectangles_dimensions = new int[2];
                Rectangles_dimensions[0]= _graphics.PreferredBackBufferWidth/16;
                Rectangles_dimensions[1]= _graphics.PreferredBackBufferHeight/6;
                x_impulse = 0;
                speed = Cube_dimensions[0]/10;
                next_obstacle = 0;
                Bg_rect = [];


            }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.DrawString(scores_font, "Score:"+score, new Vector2(10, 10), Color.Black);
        _spriteBatch.DrawString(scores_font, "Highscore:"+highscore, new Vector2(120, 10), Color.Black);
        _spriteBatch.DrawString(scores_font, "Difficulty:"+speed/7+"."+speed%7, new Vector2(280, 10), Color.Black);

        if(double_jump)
            _spriteBatch.Draw(cubedj ,new Rectangle(Cube_position[0],Cube_position[1],Cube_dimensions[0],Cube_dimensions[1]), Color.OldLace);
        else
            _spriteBatch.Draw(cube ,new Rectangle(Cube_position[0],Cube_position[1],Cube_dimensions[0],Cube_dimensions[1]), Color.OldLace);
        // Drawn rectangle
        for (int i= 0;i < Bg_rect.Count;i ++)
        {   _spriteBatch.Draw(Bg_rect[i].texture,new Rectangle(Bg_rect[i].x_pos,Bg_rect[i].y_pos-Rectangles_dimensions[1],Rectangles_dimensions[0],Rectangles_dimensions[1]), Color.OldLace);}
        
        // Drawn soil
        DrawLine(new Vector2(0, _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4), new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4), 5, Color.Brown);
        if (died)
        _spriteBatch.DrawString(scores_font, "You died. To play again press ENTER, to quit press ESC", new Vector2(_graphics.PreferredBackBufferWidth/2-300, _graphics.PreferredBackBufferHeight/2), Color.Black);

        _spriteBatch.End();
        

        base.Draw(gameTime);
    }
    private bool CubeIsSuported()
    {
        
        return Cube_position[1] == _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;

    }
    public Texture2D ChangeColorToTransparent(GraphicsDevice graphicsDevice, Texture2D texture, Color targetColor)
    {
    Color[] data = new Color[texture.Width * texture.Height];
    texture.GetData(data);

    for (int i = 0; i < data.Length; i++)
    {
        if (data[i].R == targetColor.R && data[i].G == targetColor.G && data[i].B == targetColor.B)
        {   
            data[i].R = 0;
            data[i].G = 0;
            data[i].B = 0;
            data[i].A = 0; // Set alpha to 0 for transparency
        }
    }

    Texture2D newTexture = new Texture2D(graphicsDevice, texture.Width, texture.Height);
    newTexture.SetData(data);
    
    return newTexture;
    }
    private void DrawLine(Vector2 start, Vector2 end, float thickness, Color color)
    {
        Vector2 direction = end - start;
        float length = direction.Length();
        direction.Normalize();

        // Calculate the rotation angle
        float angle = (float)Math.Acos(Vector2.Dot(Vector2.UnitX, direction));

        // Adjust angle based on the direction of Y
        if (direction.Y < 0)
            angle = MathHelper.TwoPi - angle;

        // Draw the line
        _spriteBatch.Draw(pixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);
    }
    
}
public class BadGuyRectangle
{
    public int x_pos {get; set;}
    public int y_pos {get; set;}
    
    public Texture2D texture {get; set;}
    public BadGuyRectangle(int x_pos, int y_pos,Texture2D texture)
    {
        this.x_pos = x_pos;
        this.y_pos = y_pos;
        
        this.texture = texture;
    }

}
