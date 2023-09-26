# ComfyEconomy
A TShock economy plugin.

## Permissions
| Permissions        | Commands               |
|--------------------|------------------------|
| comfyeco.bal.check | bal, balance           |
| comfyeco.bal.admin | baladmin, balanceadmin |
| comfyeco.pay       | pay                    |
| comfyeco.addmine   | addmine                |
| comfyeco.updateeco | updateeco              |

| Permissions         | Function                                                          |
|---------------------|-------------------------------------------------------------------|
| comfyeco.serversign | To be able to place server signs (-S-Buy-, -S-Sell-, -S-Command-) |

## Shop Sign Syntax
* For Item Signs (-Buy-, -S-Buy-, -S-Sell-):
```
Tag
Item Name or ID
Amount
Price
```
or
```
Tag; Item Name or ID; Amount; Price
```
* For Command Signs (-S-Command-):
```
Tag
Command
Description
Price
```
or
```
Tag; Command; Description; Price
```
**Note**: _If you're updating to v.1.3.0 from an earlier version, you can update shop sign formatting with ``/updateeco``._ <br>
          _Using this command multiple times will break the shop signs._ <br>
          _If you're not updating from an earlier version you don't need to use this command. If you do it'll break the shop signs._
