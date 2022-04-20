using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script para animaciones de NPC 
public class CustomAnimator
{
    SpriteRenderer renderer;
    List<Sprite> animFrames;    
    float frameRate;

    int currentFrame;
    float timer;

    public List<Sprite> AnimFrames => animFrames;
    public CustomAnimator(SpriteRenderer renderer, List<Sprite> animFrames, float frameRate = 0.125f)
    {
        this.renderer = renderer;
        this.animFrames = animFrames;
        this.frameRate = frameRate;
    }

    //no es el de unity, es el de iniciar animación
    public void Start()
	{
        currentFrame = 0;
        timer = 0f;
        renderer.sprite = animFrames[currentFrame];
	}

    //es propio, no el de unity
    public void HandleUpdate()
	{
        timer += Time.deltaTime;
        if (timer > frameRate)
        {
            currentFrame =( currentFrame+1)% animFrames.Count;
            renderer.sprite = animFrames[currentFrame];
            //restamos el tiempo para una animación más exacta
            timer -= frameRate;            
        }
    }
}
