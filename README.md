# ComfyEconomy
A TShock economy plugin.

## Permissions
| Permissions        | Commands               |
|--------------------|------------------------|
| comfyeco.bal.check | bal, balance           |
| comfyeco.bal.admin | baladmin, balanceadmin |
| comfyeco.pay       | pay                    |
| comfyeco.addmine   | addmine                |

| Permissions         | Function                                                          |
|---------------------|-------------------------------------------------------------------|
| comfyeco.serversign | To be able to place server signs (-S-Buy-, -S-Sell-, -S-Command-, -S-Trade-) |

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

