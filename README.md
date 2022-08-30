# Retexture Pack

This unofficial TaleSpire pluigin allows mini and effects to be retextured allowing the same asset to be used without
different textures. Great for situations such as turning minis into statues, apply colours to signify conditions such
as stealth, and for applying situation specific texture to signs, flags, etc.

While the plugin does not come with the flag pack, it can be downloaed separately and this plugin can be used to change 
the contents of the flag.

## Change Log
```
2.2.0: Added video support
2.1.0: Improved hierarchy traversing code to work with different asset structures
2.0.0: Fix for BR HF Integration update
1.0.1: Added subscribe on start so that RCTRL+Y is not necessary if making a new board
1.0.0: Initial release
```
## Install

Use R2ModMan or similar installer to install this asset pack.

## Usage

1. Click on a mini
2. Activate the Re-texture feature using the corresponding keyboard shortcut (default RCTRL+X)
3. In the dialog that opens, enter the name of the alternate texture (including extension)

If the mini has a effect applied, the texture is retextured. If the mini does not have a effect applied, the mini is
retextured instead. If the target (mini or effect) has a material called RETEXTURE_MAT then this material texture is
replaced. If the target does not have a material with this name then the default material is retextured.

## Retexturing Minis

Most TS core minis use a single texture and thus are compatible with the Retexture plugin using the default material
texture. However, some core TS mini use multiple textures in which case they will not be fully compatilble with the
Retexture plugin (i.e. only part of the mini texture may be changed).

## Retexturing Effects

If an effect uses a single texture then nothing special needs to be done when creating the effect in order to allowing
the Retecture plugin to retexture the material. However, if the effect uses multiple materials then desired material
for Retextuirng needs to be called RETEXTURE_MAT. In this way, the asset tells the Retexture plugin which material
texture to change.

## Limitations

Due to the removal of Effect kind in CALP in favor of using the SHIV Transparency Trick, the transparent features of
the Retexture plugin will only work with assets that properly implement the SHIV Transparency Trick to hide the part
of the asset that is transparent from core TS.

Currently the Flat Token pack has not been updated to the SHIV Transparency Trick and thus the transparent version of
the flat tokens will not work. However, the solid flat token (and the coin) can be retextured.
