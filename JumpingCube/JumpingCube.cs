using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Net.Mime;
using System.Collections.Generic;

namespace JumpingCube;

public class JumpingCube : Game
{   
    private Texture2D cube;
    private Texture2D pixel;

    private Texture2D rectangle;
    private int cube_position { get; set; }
    private int speed { get; set;}
    private List<BadGuyRectangle> bg_rect {get; set;}
    private int x_impulse { get; set; }
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private int next_obstacle {get; set;}
    public JumpingCube()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        cube_position = _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;
        x_impulse = 0;
        speed = 4;
        next_obstacle = 0;
        bg_rect = [];
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        cube = Content.Load<Texture2D>("cube");
        cube = ChangeColorToTransparent(GraphicsDevice,cube,Color.White);
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        rectangle = Content.Load<Texture2D>("rectangle");
        Color[] data = new Color[1];
        data[0] = Color.White; 
        pixel.SetData(data);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (cube_position + x_impulse > _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10)
            cube_position = _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;
        else
            cube_position += x_impulse;

        if (cube_position >=  _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 )
            cube_position = _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;
        if (x_impulse <= 7)
        //if (x_impulse <= 0 && !(x_impulse >7))
            x_impulse +=1;
        
        if (CubeIsSuported())
            x_impulse = 0;

        if (CubeIsSuported() && Keyboard.GetState().IsKeyDown(Keys.Space))
            x_impulse = -18;
        if (bg_rect != null)
        for (int i= 0;i < bg_rect.Count;i ++)
        {
            bg_rect[i].x_pos -= speed;
            if (bg_rect[i].x_pos<-70)
                bg_rect.RemoveAt(i);

        }
        next_obstacle-= speed;
        if (next_obstacle <= 0)
        {
            Random random = new Random();
            int randm = random.Next(1,30);
            if (randm >= 30-speed)
            {
                bg_rect.Add(new BadGuyRectangle(_graphics.PreferredBackBufferWidth,_graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4,rectangle));
                int randm_gap = random.Next(1,50);
                next_obstacle = 180 + randm_gap;
                
            }

        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        // Drawn rectangle
        _spriteBatch.Draw(cube ,new Rectangle(10,cube_position,_graphics.PreferredBackBufferHeight/10,_graphics.PreferredBackBufferHeight/10), Color.OldLace);
        
        for (int i= 0;i < bg_rect.Count;i ++)
        {   _spriteBatch.Draw(bg_rect[i].texture,new Rectangle(bg_rect[i].x_pos,bg_rect[i].y_pos-_graphics.PreferredBackBufferHeight/8,_graphics.PreferredBackBufferWidth/16,_graphics.PreferredBackBufferHeight/8), Color.OldLace);}
        
        // Drawn soil
        DrawLine(new Vector2(0, _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4), new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4), 5, Color.Brown);
        _spriteBatch.End();
        

        base.Draw(gameTime);
    }
    private bool CubeIsSuported()
    {
        
        return cube_position == _graphics.PreferredBackBufferHeight-_graphics.PreferredBackBufferHeight/4 - _graphics.PreferredBackBufferHeight/10;

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

    //public void Draw (int widht, int height,SpriteBatch _spriteBatch)
    //_spriteBatch.Draw(texture ,new Rectangle(x_pos,y_pos,_graphics.PreferredBackBufferHeight/10,_graphics.PreferredBackBufferHeight/10), Color.OldLace);
    
}
