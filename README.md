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
| comfyeco.serversign | To be able to place server signs (-S-Buy-, -S-Sell-, -S-Command-, -S-Trade-) |

**Note**: _If you're updating to v.1.3.0 from an earlier version, you can update shop sign formatting with ``/updateeco``._ <br>
          _Using this command multiple times will break the shop signs._ <br>
          _If you're not updating from an earlier version you don't need to use this command. If you do it'll break the shop signs._

## Shop Sign Syntax
### Item Signs (-Buy-, -S-Buy-, -S-Sell-)
```
Tag
Item Name or ID
Amount
Price
```
### Command Sign (-S-Command-)
```
Tag
Command
Description
Price
```
### Trade Sign (-S-Trade-)
```
Tag
Item Name or ID
Amount
Required Item Name or ID
Required Amount
```
**Note**: _``Tag`` refers to sign's type such as -Buy-, -S-Command-, etc._ <br>
**Note**: _Since mobile players can't type in new line character, they would need to use this kind of syntax instead:_
```
Tag; Item Name or ID; Amount; Price
```


### Examples
* Default (-Buy-)
<img src="https://github.com/Soof4/ComfyEconomy/blob/main/Shop%20Sign%20Syntax%20Examples/default0.png?raw=true" alt="alt text" height="160px">
<img src="https://github.com/Soof4/ComfyEconomy/blob/main/Shop%20Sign%20Syntax%20Examples/default1.png?raw=true" alt="alt text" height="160px">

* Server (-S-Buy-, -S-Sell-)
<img src="https://github.com/Soof4/ComfyEconomy/blob/main/Shop%20Sign%20Syntax%20Examples/server0.png?raw=true" alt="alt text" height="160px">
<img src="https://github.com/Soof4/ComfyEconomy/blob/main/Shop%20Sign%20Syntax%20Examples/server1.png?raw=true" alt="alt text" height="160px">

* Command (-S-Command-)
<img src="https://github.com/Soof4/ComfyEconomy/blob/main/Shop%20Sign%20Syntax%20Examples/command0.png?raw=true" alt="alt text" height="160px">
<img src="https://github.com/Soof4/ComfyEconomy/blob/main/Shop%20Sign%20Syntax%20Examples/command1.png?raw=true" alt="alt text" height="160px">

