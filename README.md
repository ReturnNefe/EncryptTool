# EncryTool

这是一个简易的加密工具套，支持：
* **EncryConsole/EncryText** : RSA & AES 文本/文件的加密
* **PicMaker** : 文件/文本与图片互转

## 技术

### 引用的库

| 名称 | 引用此库的项目 | 协议 | 仓库地址 |
| -- | -- | -- | -- |
| RetChen.Encryption | EncryText, EncryConsole | MIT | |
| MetroModernUI | EncryText | MIT | http://denricdenise.info/ |

### 框架

| 项目 | 框架 |
| -- | -- |
| EncryConsole | .net 6.0 |
| EncryText | .net framework 4.8 |
| PicMaker | .net 6.0 |

## 效果

![EncryText](https://github.com/Return25/EncryTool/blob/main/screenshot/encrytext.png)

加密文本

![PicMaker](https://github.com/Return25/EncryTool/blob/main/screenshot/picmaker.png)

将 mp3 文件转换为 png

## 使用

### 安装

下载右侧 Releases 中的 EncryTool.zip。
目前仅 `EncryConsole` 可在其他 OS 中运行，如果让 `PicMaker` 跨平台，可以将 `System.Drawing.Common(GDI+)` 更换为 `OpenCV`。

### PicMaker

支持 `UTF-8 文本` / `文件` 与图片互相转换。

#### 转换文本：
`picmaker -et <文本文件> <输出的图片>`
`picmaker -dt <图片> <输出的文本>`
#### 转换文件：
`picmaker -e <文件> <输出的图片>`
`picmaker -d <图片> <输出的文件>`

更多命令，可以输入 `picmaker -h` 查看
*如果你的文件文件不是 **UTF-8** , 请使用 `-e` 而不是 `-ef`*

### EncryConsole

> 如果你不了解 RSA 和 AES 算法，可以浏览：
[RSA - CTF](https://ctf-wiki.org/crypto/asymmetric/rsa/rsa_theory/)
[AES - CTF](https://ctf-wiki.org/crypto/blockcipher/aes/)

支持使用 AES 与 RSA 算法混合加密文件。

#### 生成密钥：
使用前，请确保你有一对 RSA Key：
`encryconsole -m <密钥长度>`
该命令会生成一对密钥（公钥和私钥），请妥善保存。

*密钥长度：不应太小，否则会有被攻击的危险，建议为 4096*

#### 加密文件：
`encryconsole -e <待加密的文件> <加密后的文件>`
输入RSA公钥，回车，当出现 `Success` 字样时，代表加密完成。
同时，该命令会生成一个 AES密钥（已加密），请妥善保存。

#### 解密文件：
`encryconsole -d <待解密的文件> <解密后的文件>`
输入RSA密钥，回车，接着输入刚才的AES密钥，回车。
当出现 `Success` 字样时，代表解密完成。

#### *使用场景*
有时候，你可能需要在某些平台传输一些重要的资料，如果想要避免泄露，可以使用 `EncryConsole`。
首先，你和对方需要各自执行 `encryconsole -m <密钥长度>`，然后将得到的公钥（Public Key）发送给对方。
加密时，使用对方的公钥进行加密，然后将得到的AES密钥发送给对方。
对方收到AES密钥和加密后的文件之后，通过自己的密钥和收到的AES密钥解密。

*Q： 为什么会用到 AES？*
*A：纯RSA的计算时间太长，因此仅使用RSA加密AES密钥，由AES算法对数据进行加密。*

### EncryText

原理和 `EncryConsole` 类似，但只支持加密文本。
附带 GUI。

## 贡献

此项目欢迎任何人的 Pull Request 和 Issue，也欢迎 Star 和 Fork。