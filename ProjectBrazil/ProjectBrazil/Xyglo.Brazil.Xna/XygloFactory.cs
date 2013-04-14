using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xyglo.Brazil.Xna.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jitter;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using Xyglo.Friendlier;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Generate drawables and physics attachments for all the Brazil components that we accept
    /// </summary>
    public class XygloFactory
    {
        /// <summary>
        /// Construct this factory with some useful handles
        /// </summary>
        /// <param name="brazilContext"></param>
        /// <param name="context"></param>
        /// <param name="physicsHandler"></param>
        /// <param name="eyeHandler"></param>
        /// <param name="mouse"></param>
        /// <param name="frameCounter"></param>
        public XygloFactory(XygloContext context, BrazilContext brazilContext, EyeHandler eyeHandler, XygloMouse mouse, FrameCounter frameCounter)
        {
            m_brazilContext = brazilContext;
            m_context = context;
            m_eyeHandler = eyeHandler;
            m_mouse = mouse;
            m_frameCounter = frameCounter;
        }

        /// <summary>
        /// Turn a BrazilComponent into a XygloDrawable for the first time and
        /// add it to the list of drawables.
        /// </summary>
        /// <param name="component"></param>
        public void createInitialXygloDrawable(BrazilView view, Component component)
        {
            // Ignore all but 3D components at this stage
            //
            //if (component.GetType() != typeof(Component3D))
            //continue;

            // Default translation and multipler
            //
            Vector3 viewTranslation = Vector3.Zero;
            float multiplier = 1.0f; 

            // Adjust by any view if one is passed
            //
            if (view != null && view.getApp().getWorldBounds() != null)
            {
                // Translate
                viewTranslation += view.getPosition();

                // Scale
                double xMult = view.getApp().getWorldBounds().getWidth() / view.getWidth();
                double yMult = view.getApp().getWorldBounds().getHeight() / view.getHeight();
                multiplier = Math.Min((float)xMult, (float)yMult);
            }

            // If not then is it a drawable type? 
            //
            if (component.GetType() == typeof(Xyglo.Brazil.BrazilFlyingBlock))
            {
                // Found a FlyingBlock - initialise it and add it to the dictionary
                //
                BrazilFlyingBlock fb = (Xyglo.Brazil.BrazilFlyingBlock)component;

                // Allow for container view
                Vector3 position = XygloConvert.getVector3(fb.getPosition());
                Vector3 size = XygloConvert.getVector3(fb.getSize());

                // If we're running in a container then move the position of the item and
                // scale by the relative size of the worlds.
                //
                /*
                if (view != null && view.getApp().getWorldBounds() != null)
                {
                    // Translate
                    position += view.getPosition();

                    // Scale
                    double xMult = view.getApp().getWorldBounds().getWidth() / view.getWidth();
                    double yMult = view.getApp().getWorldBounds().getHeight() / view.getHeight();
                    multiplier = Math.Min((float)xMult, (float)yMult);
                    size *= multiplier;
                }*/

                /*
                foreach(ResourceInstance resource in fb.getResources())
                {
                    switch (resource.getResource().getType())
                    {

                    }
                }*/

                // If we have an image attached to this component then draw it with the image
                //
                if (fb.getResourceByType(ResourceType.Image).Count > 0 && m_context.m_xygloResourceMap.Count() > 0)
                {
                    // Get the XygloResource using the unique name
                    //
                    XygloImageResource xir = (XygloImageResource)m_context.m_xygloResourceMap[fb.getResources()[0].getResource().getName()];
                    m_context.m_physicsEffect.Texture = xir.getTexture();

                    XygloTexturedBlock drawBlock = new XygloTexturedBlock(XygloConvert.getColour(fb.getColour()), m_context.m_physicsEffect, viewTranslation + XygloConvert.getVector3(fb.getPosition()) * multiplier, XygloConvert.getVector3(fb.getSize()) * multiplier);
                    drawBlock.setVelocity(XygloConvert.getVector3(fb.getVelocity()) * multiplier);

                    // Naming is useful for tracking these blocks
                    drawBlock.setName(fb.getName());

                    // Set any rotation amount
                    drawBlock.setRotation(fb.getInitialAngle());

                    // Initial build and draw
                    //
                    drawBlock.buildBuffers(m_context.m_graphics.GraphicsDevice);
                    drawBlock.draw(m_context.m_graphics.GraphicsDevice);

                    // Push to dictionary
                    //
                    m_context.m_drawableComponents[component] = drawBlock;

                    // Add to the physics handler if we need to
                    //
                    //m_physicsHandler.
                    createPhysical(component, drawBlock);
                }
                else
                {
                    // Draw it without a texture
                    //
                    XygloFlyingBlock drawBlock = new XygloFlyingBlock(XygloConvert.getColour(fb.getColour()), m_context.m_lineEffect, viewTranslation + position * multiplier, size * multiplier);
                    drawBlock.setVelocity(XygloConvert.getVector3(fb.getVelocity()));

                    // Naming is useful for tracking these blocks
                    drawBlock.setName(fb.getName());

                    // Set any rotation amount
                    drawBlock.setRotation(fb.getInitialAngle());

                    // Initial build and draw
                    //
                    drawBlock.buildBuffers(m_context.m_graphics.GraphicsDevice);
                    drawBlock.draw(m_context.m_graphics.GraphicsDevice);

                    // Push to dictionary
                    //
                    m_context.m_drawableComponents[component] = drawBlock;

                    // Add to the physics handler if we need to
                    //
                    //m_physicsHandler.
                    createPhysical(component, drawBlock);
                }

            }
            else if (component.GetType() == typeof(Xyglo.Brazil.BrazilInterloper))
            {
                BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)component;

                XygloComponentGroup group = new XygloComponentGroup(XygloComponentGroupType.Interloper, m_context.m_lineEffect, Vector3.Zero);
                group.setPosition(viewTranslation + XygloConvert.getVector3(il.getPosition()) * multiplier);

                XygloFlyingBlock drawBlock = new XygloFlyingBlock(XygloConvert.getColour(il.getColour()), m_context.m_lineEffect, viewTranslation + XygloConvert.getVector3(il.getPosition()) * multiplier, XygloConvert.getVector3(il.getSize()) * multiplier);
                group.addComponent(drawBlock);

                // Set the name of the component group from the interloper
                //
                group.setName(il.getName());

                XygloSphere drawSphere = new XygloSphere(XygloConvert.getColour(il.getColour()), m_context.m_lineEffect, viewTranslation + XygloConvert.getVector3(il.getPosition()) * multiplier, il.getSize().X * multiplier);
                drawSphere.setRotation(il.getRotation());
                group.addComponentRelative(drawSphere, new Vector3(0, -(float)il.getSize().X, 0));

                group.buildBuffers(m_context.m_graphics.GraphicsDevice);
                group.draw(m_context.m_graphics.GraphicsDevice);

                //group.setVelocity(new Vector3(0.01f, 0, 0));

                group.setVelocity(XygloConvert.getVector3(il.getVelocity()) * multiplier);
                m_context.m_drawableComponents[component] = group;

                //m_physicsHandler.
                createPhysical(component, group);
            }
            else if (component.GetType() == typeof(Xyglo.Brazil.BrazilBannerText))
            {
                // A BrazilBanner we have to draw according to app mode - if we're
                // hosting a container then draw the banner within the container else
                // we use the whole screen.
                //
                BrazilBannerText bt = (Xyglo.Brazil.BrazilBannerText)component;
                XygloBannerText bannerText = null;

                // The helper method does all the hard work in getting this position
                //
                if (view == null)
                {
                    Vector3 position = XygloConvert.getTextPosition(bt, m_context.m_fontManager, m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height);
                    bannerText = new XygloBannerText("banner1", m_context.m_overlaySpriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(bt.getColour()), position, bt.getSize(), bt.getText());
                }
                else
                {
                    Vector3 position = viewTranslation + XygloConvert.getComponentRelativePosition(bt, view);
                    bannerText = new XygloBannerText("banner2", m_context.m_spriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(bt.getColour()), position, bt.getSize() * multiplier, bt.getText());
                }

                m_context.m_drawableComponents[component] = bannerText;
                
            }
            else if (component.GetType() == typeof(Xyglo.Brazil.BrazilHud))
            {
                // Only create a HUD for a top level app
                //
                if (component.getApp() == null)
                {
                    BrazilHud bh = (Xyglo.Brazil.BrazilHud)component;
                    Vector3 position = XygloConvert.getVector3(bh.getPosition());

                    //if (bh.getApp() == null)
                    //{
                    string bannerString = "";

                    if (component.getApp() == null && m_frameCounter.getFrameRate() > 0)
                        bannerString += "FPS = " + m_frameCounter.getFrameRate() + "\n";

                    if (m_context.m_project != null)
                    {
                        bannerString += "[EyePosition] X " + m_eyeHandler.getEyePosition().X + ",Y " + m_eyeHandler.getEyePosition().Y + ",Z " + m_eyeHandler.getEyePosition().Z + "\n";
                    }
                    else if (m_brazilContext.m_interloper != null)
                    {
                        // Interloper position
                        //
                        //Vector3 ipPos = m_context.m_drawableComponents[m_brazilContext.m_interloper].getPosition();
                        //bannerString += "Interloper Position X = " + ipPos.X + ", Y = " + ipPos.Y + ", Z = " + ipPos.Z + "\n";

                        // Interloper score
                        //
                        bannerString += "Score = " + m_brazilContext.m_interloper.getScore() + "\n";
                        bannerString += "Lives = " + m_brazilContext.m_world.getLives() + "\n";
                    }

                    XygloBannerText bannerText = new XygloBannerText("hug", m_context.m_overlaySpriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(bh.getColour()), position, bh.getSize(), bannerString);
                    m_context.m_drawableComponents[component] = bannerText;
                    //}
                }
            }
            else if (component.GetType() == typeof(Xyglo.Brazil.BrazilGoody))
            {
                BrazilGoody bg = (BrazilGoody)component;

                if (bg.m_type == BrazilGoodyType.Coin)
                {
                    // Build a coin
                    //
                    XygloCoin coin = new XygloCoin(Color.Yellow, m_context.m_lineEffect, viewTranslation + XygloConvert.getVector3(bg.getPosition()) * multiplier, bg.getSize().X * multiplier);
                    coin.setRotation(bg.getRotation());
                    coin.buildBuffers(m_context.m_graphics.GraphicsDevice);
                    coin.draw(m_context.m_graphics.GraphicsDevice, FillMode.Solid);

                    // And store in drawable component array
                    //
                    m_context.m_drawableComponents[component] = coin;

                    //m_physicsHandler.
                    createPhysical(component, coin);
                }
                else
                {
                    throw new XygloException("Update", "Unsupported Goody Type");
                }
            }
            else if (component.GetType() == typeof(Xyglo.Brazil.BrazilBaddy))
            {
                Logger.logMsg("Draw Baddy for the first time");

                BrazilBaddy baddy = (BrazilBaddy)component;

                XygloComponentGroup group = new XygloComponentGroup(XygloComponentGroupType.Fiend, m_context.m_lineEffect, Vector3.Zero);
                group.setPosition(viewTranslation + XygloConvert.getVector3(baddy.getPosition()) * multiplier);

                XygloFlyingBlock drawBlock = new XygloFlyingBlock(XygloConvert.getColour(baddy.getColour()), m_context.m_lineEffect, viewTranslation + XygloConvert.getVector3(baddy.getPosition()) * multiplier, XygloConvert.getVector3(baddy.getSize()) * multiplier);
                group.addComponent(drawBlock);

                // Set the name of the component group from the interloper
                //
                group.setName(baddy.getName());

                XygloSphere drawSphere = new XygloSphere(XygloConvert.getColour(baddy.getColour()), m_context.m_lineEffect, viewTranslation + XygloConvert.getVector3(baddy.getPosition()) * multiplier, baddy.getSize().X * multiplier);
                drawSphere.setRotation(baddy.getRotation());
                group.addComponentRelative(drawSphere, new Vector3(0, -(float)baddy.getSize().X, 0));

                group.buildBuffers(m_context.m_graphics.GraphicsDevice);
                group.draw(m_context.m_graphics.GraphicsDevice);

                //group.setVelocity(new Vector3(0.01f, 0, 0));

                group.setVelocity(XygloConvert.getVector3(baddy.getVelocity()));
                m_context.m_drawableComponents[component] = group;

                //m_physicsHandler.
                createPhysical(component, group);
            }
            else if (component.GetType() == typeof(Xyglo.Brazil.BrazilFinishBlock))
            {
                BrazilFinishBlock fb = (BrazilFinishBlock)component;
                // Allow for container view
                Vector3 position = XygloConvert.getVector3(fb.getPosition());
                Vector3 size = XygloConvert.getVector3(fb.getSize());
                //float multiplier = 1.0f;

                //Logger.logMsg("Draw Finish Block for the first time");
                // Draw it without a texture
                //
                XygloFinishBlock drawBlock = new XygloFinishBlock(XygloConvert.getColour(fb.getColour()), m_context.m_lineEffect, position, size);
                drawBlock.setVelocity(XygloConvert.getVector3(fb.getVelocity()));

                // Naming is useful for tracking these blocks
                drawBlock.setName(fb.getName());

                // Set any rotation amount
                drawBlock.setRotation(fb.getInitialAngle());

                // Initial build and draw
                //
                drawBlock.buildBuffers(m_context.m_graphics.GraphicsDevice);
                drawBlock.draw(m_context.m_graphics.GraphicsDevice);

                // Push to dictionary
                //
                m_context.m_drawableComponents[component] = drawBlock;

                // Add to the physics handler if we need to
                //
                //m_physicsHandler.
                createPhysical(component, drawBlock);

            }
            else if (component.GetType() == typeof(Xyglo.Brazil.BrazilMenu))
            {
                BrazilMenu bMenu = (BrazilMenu)component;

                // Line effect or Basic effect here?
                //
                XygloMenu menu = new XygloMenu(m_context.m_fontManager, m_context.m_spriteBatch, Color.DarkGray, m_context.m_lineEffect, m_mouse.getLastClickWorldPosition(), m_mouse.geLastClickCursorOffset(), m_context.m_project.getSelectedView().getViewSize());

                foreach (BrazilMenuOption item in bMenu.getMenuOptions().Keys)
                {
                    menu.addOption(item.m_optionName);
                }

                // Build the buffers and draw
                //
                menu.buildBuffers(m_context.m_graphics.GraphicsDevice);
                menu.draw(m_context.m_graphics.GraphicsDevice);
                m_context.m_drawableComponents[component] = menu;
            }
            else if (component.GetType() == typeof(BrazilTestBlock))
            {
                BrazilTestBlock bTB = (BrazilTestBlock)component;
                XygloTexturedBlock block = new XygloTexturedBlock(XygloConvert.getColour(bTB.getColour()), m_context.m_physicsEffect, viewTranslation + XygloConvert.getVector3(bTB.getPosition()) * multiplier, XygloConvert.getVector3(bTB.getSize()) * multiplier);

                block.buildBuffers(m_context.m_graphics.GraphicsDevice);
                block.draw(m_context.m_graphics.GraphicsDevice);
                m_context.m_drawableComponents[component] = block;

                //m_physicsHandler.
                createPhysical(component, block);
            }
            else if (component.GetType() == typeof(BrazilInvisibleBlock))
            {
                createPhysical((BrazilInvisibleBlock)component);
            }
            else
            {
                throw new XygloException("Update", "Unsupported Brazil Type in XygloFactory");
            }
        }

        /// <summary>
        /// Create an invisible (physical only) item from a component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public RigidBody createPhysical(BrazilInvisibleBlock block)
        {
            RigidBody body = null;
            body = new RigidBody(new BoxShape(Conversion.ToJitterVector(XygloConvert.getVector3(block.getSize()))));
            body.Position = Conversion.ToJitterVector(XygloConvert.getVector3(block.getPosition()));
            body.IsStatic = true;
            body.Mass = 10000;
            m_context.m_physicsHandler.addRigidBody(body);

            return body;
        }

        /// <summary>
        /// Interpret a Drawable and add it to the Physics model according to type
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="affectedByGravity"></param>
        /// <param name="moveable"></param>
        public RigidBody createPhysical(Component component, XygloXnaDrawable drawable)
        {
            RigidBody body = null;

            if (drawable is XygloFlyingBlock)
            {
                XygloFlyingBlock fb = (XygloFlyingBlock)drawable;
                body = new RigidBody(new BoxShape(Conversion.ToJitterVector(fb.getSize())));
            }
            else if (drawable is XygloTexturedBlock)
            {
                XygloTexturedBlock fb = (XygloTexturedBlock)drawable;
                JVector size = Conversion.ToJitterVector(fb.getSize());
                body = new RigidBody(new BoxShape(size));
            }
            else if (drawable is XygloSphere)
            {
                XygloSphere sphere = (XygloSphere)drawable;
                body = new RigidBody(new SphereShape(sphere.getRadius()));
            }
            else if (drawable is XygloComponentGroup)
            {
                createPhysicalComponentGroup(component, (XygloComponentGroup)drawable);
            }

            // If we've constructed a body then populate and add
            //
            if (body != null)
            {
                body.Position = Conversion.ToJitterVector(drawable.getPosition());
                body.AffectedByGravity = component.isAffectedByGravity();
                body.IsStatic = !component.isMoveable();
                body.Mass = 1.0f; //  Math.Max(component.getMass(), 1000);

                // Set a velocity if we're not static
                //
                if (!body.IsStatic)
                    body.LinearVelocity = Conversion.ToJitterVector(drawable.getVelocity());

                // Store this relationship in the calling drawable so we can link them back again
                //
                drawable.setPhysicsHash(body.GetHashCode());

                body.EnableSpeculativeContacts = true;

                // set restitution
                body.Material.Restitution = 0.1f; //  component.getHardness();
                //body.LinearVelocity = new JVector(0, 0, 0);  
                body.Damping = RigidBody.DampingType.Angular;

                //World.AddBody(body);
                m_context.m_physicsHandler.addRigidBody(body);


                //sphere.Position = boxPos + JVector.Up * 30;
                //sphere.EnableSpeculativeContacts = true;

                // set restitution
                //sphere.Material.Restitution = box.Material.Restitution = 1.0f / 10.0f * i;
                //sphere.LinearVelocity = new JVector(0, 20, 0);


                //sphere.Damping = RigidBody.DampingType.Angular;

                return body;
            }
            else
            {
                Logger.logMsg("Not constructed a physics objects from a XygloDrawable");
            }

            return null;
        }


        /// <summary>
        /// Link a set of components to a component group and perform some coupling
        /// between them.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="group"></param>
        protected void createPhysicalComponentGroup(Component component, XygloComponentGroup group)
        {

            if (group.getComponentGroupType() == XygloComponentGroupType.Interloper)
            {
                XygloXnaDrawable headDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloSphere)).ToList()[0];
                RigidBody head = createPhysical(component, headDrawable);

                // Stop rotations  - this might be wrong!
                //
                //head.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);
                head.Material.Restitution = component.getHardness();
                head.Damping = RigidBody.DampingType.Angular;
                head.Mass = component.getMass();
                head.EnableSpeculativeContacts = true;
//                head.SetMassProperties(
                //body.LinearVelocity = new JVector(0, 0, 0);  
                

                XygloXnaDrawable bodyDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloFlyingBlock)).ToList()[0];
                RigidBody body = createPhysical(component, bodyDrawable);

                // See above caveat!
                //
                body.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);
                body.Material.Restitution = component.getHardness();
                body.Damping = RigidBody.DampingType.Angular;
                body.Mass = component.getMass();
                body.EnableSpeculativeContacts = true;
                // Connect head and torso with a hard point to point connection like so
                //
                PointPointDistance headTorso = new PointPointDistance(head, body, head.Position, body.Position);
                headTorso.Softness = 0.00001f;

                // Add the connection - the body parts are already add implicitly (might want to change that)
                //
                m_context.m_physicsHandler.addConstraint(headTorso);

                //sphere.EnableSpeculativeContacts = true;

                // set restitution
                //sphere.Material.Restitution = box.Material.Restitution = 1.0f / 10.0f * i;
                //sphere.LinearVelocity = new JVector(0, 20, 0);


                //sphere.Damping = RigidBody.DampingType.Angular;

                // Special value for collection to indicate it
                //
                group.setPhysicsHash(-1);
            }
            else if (group.getComponentGroupType() == XygloComponentGroupType.Fiend)
            {
                XygloXnaDrawable headDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloSphere)).ToList()[0];
                RigidBody head = createPhysical(component, headDrawable);

                // Stop rotations  - this might be wrong!
                //
                head.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);

                XygloXnaDrawable bodyDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloFlyingBlock)).ToList()[0];
                RigidBody body = createPhysical(component, bodyDrawable);

                // See above caveat!
                //
                body.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);

                // Connect head and torso with a hard point to point connection like so
                //
                PointPointDistance headTorso = new PointPointDistance(head, body, head.Position, body.Position);
                headTorso.Softness = 0.00001f;

                // Add the connection - the body parts are already add implicitly (might want to change that)
                //
                m_context.m_physicsHandler.addConstraint(headTorso);

                //XygloComponentGroup group = (XygloComponentGroup)drawable;
                //foreach (XygloXnaDrawable subDrawable in group.getComponents())
                //{
                //createPhysical(component, subDrawable);
                //}

                // Special value for collection to indicate it
                //
                group.setPhysicsHash(-1);
            }
        }

        /// <summary>
        /// The XygloContext
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// The BrazilContext - game context
        /// </summary>
        protected BrazilContext m_brazilContext;

        /// <summary>
        /// Eye handler
        /// </summary>
        protected EyeHandler m_eyeHandler;

        /// <summary>
        /// Mouse handler
        /// </summary>
        protected XygloMouse m_mouse;

        /// <summary>
        /// XygloXNA framecounter
        /// </summary>
        protected FrameCounter m_frameCounter;
    }
}
