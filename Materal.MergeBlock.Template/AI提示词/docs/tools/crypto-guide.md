# 加解密工具使用指南

## 概述

`Materal.Utils.Crypto` 是一个加解密工具库，提供 MD5、SHA256、AES、RSA、混合加密等多种加密算法，以及 URL 编码、进制编码等实用功能。

## 安装

```bash
dotnet add package Materal.Utils.Crypto
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `Materal.Utils`（基础工具包）

## 哈希算法

### MD5Crypto - MD5 哈希

```csharp
using Materal.Utils.Crypto;

// 32位哈希（默认大写）
string hash32 = MD5Crypto.Hash32("input");

// 32位哈希（小写）
string hash32Lower = MD5Crypto.Hash32("input", isLower: true);

// 16位哈希（取32位中间部分）
string hash16 = MD5Crypto.Hash16("input");

// 字节数组哈希
string hashBytes = MD5Crypto.Hash32(data);

// 流哈希（支持大文件）
string hashStream = MD5Crypto.Hash32(stream);
```

> **注意**：MD5 已不安全，不建议用于密码存储或数字签名。

### SHA256Crypto - SHA256 哈希

```csharp
using Materal.Utils.Crypto;

// 字符串哈希
string hash = SHA256Crypto.Hash("input");

// 字节数组哈希
string hashBytes = SHA256Crypto.Hash(data);

// 流哈希
string hashStream = SHA256Crypto.Hash(stream);
```

## 对称加密

### AesCrypto - AES 加密

**CBC 模式（传统模式）**：

```csharp
using Materal.Utils.Crypto;

// 生成密钥和IV
(byte[] key, byte[] iv) = AesCrypto.GenerateCBCKey();

// 字符串加密
string encrypted = AesCrypto.CBCEncrypt("明文", key, iv);

// 字符串解密
string decrypted = AesCrypto.CBCDecrypt(encryptedData, key, iv);

// 使用随机IV（IV自动前置到密文）
string encrypted = AesCrypto.CBCEncrypt("明文", key);
string decrypted = AesCrypto.CBCDecrypt(encryptedData, key);
```

**GCM 模式（推荐，高安全性）**：

```csharp
using Materal.Utils.Crypto;

// 生成GCM密钥
string key = AesCrypto.GenerateGCMStringKey(256);

// GCM加密（自动生成nonce）
string encrypted = AesCrypto.GCMEncrypt("明文", key);

// GCM解密（自动验证完整性）
string decrypted = AesCrypto.GCMDecrypt(encryptedData, key);
```

**流式操作**：

```csharp
using Materal.Utils.Crypto;

// 文件加密
using FileStream inputStream = File.OpenRead("input.dat");
using FileStream outputStream = File.Create("encrypted.dat");
AesCrypto.CBCEncrypt(inputStream, outputStream, keyBytes, ivBytes);

// 文件解密
using FileStream inputStream = File.OpenRead("encrypted.dat");
using FileStream outputStream = File.Create("decrypted.dat");
AesCrypto.CBCDecrypt(inputStream, outputStream, keyBytes, ivBytes);
```

## 非对称加密

### RsaCrypto - RSA 加密

**生成密钥对**：

```csharp
using Materal.Utils.Crypto;

// 生成XML格式密钥对
(string publicKey, string privateKey) = RsaCrypto.GenerateKeyPair(2048);

// 生成PEM格式密钥对
(string publicKeyPem, string privateKeyPem) = RsaCrypto.GenerateKeyPairPem(2048);
```

**字符串加密/解密**：

```csharp
using Materal.Utils.Crypto;

// 加密
string encrypted = RsaCrypto.Encrypt("明文", publicKey);

// 解密
string decrypted = RsaCrypto.Decrypt(encryptedData, privateKey);
```

**大数据分块加密**：

```csharp
using Materal.Utils.Crypto;

// 加密大数据
byte[] encryptedData = RsaCrypto.EncryptLargeData(data, publicKey);

// 解密大数据
byte[] decryptedData = RsaCrypto.DecryptLargeData(encryptedData, privateKey);
```

**数字签名**：

```csharp
using Materal.Utils.Crypto;

// 签名
string signature = RsaCrypto.SignText("内容", privateKey);

// 验证签名
bool isValid = RsaCrypto.VerifyText("内容", signature, publicKey);
```

**检测密钥格式**：

```csharp
using Materal.Utils.Crypto;

KeyFormat format = RsaCrypto.DetectKeyFormat(key);
// format: Xml, PemPublic, PemPrivate, Unknown
```

## 混合加密

### HybridCrypto - RSA + AES 混合加密

适用于大数据量加密场景：

```csharp
using Materal.Utils.Crypto;

// 加密（发送方使用公钥）
byte[] encrypted = HybridCrypto.Encrypt(data, rsaPublicKey);

// 解密（接收方使用私钥）
byte[] decrypted = HybridCrypto.Decrypt(encryptedData, rsaPrivateKey);
```

**输出格式**：`[4字节密钥长度][加密的AES密钥][IV/nonce][加密数据]`

## 编码转换

### BaseCrypto - 进制编码

```csharp
using Materal.Utils.Crypto;

// 二进制编码
string binary = BaseCrypto.EncodeBinary("HELLO");

// 十六进制编码
string hex = BaseCrypto.EncodeHex("HELLO");

// 十进制编码
string decimalism = BaseCrypto.EncodeDecimalism("HELLO");

// 社会主义核心价值观编码（特色功能）
string csv = BaseCrypto.EncodeCoreSocialistValues("HELLO");

// 自定义字符集编码
string custom = BaseCrypto.Encode("HELLO", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
```

### UrlCrypto - URL 编码

```csharp
using Materal.Utils.Crypto;

// URL 编码
string encoded = UrlCrypto.Encode("参数值");

// URL 解码
string decoded = UrlCrypto.Decode(encoded);

// Base64 URL安全编码
string urlSafe = UrlCrypto.Base64UrlEncode(base64Text);
string base64 = UrlCrypto.Base64UrlDecode(urlSafeText);

// URL 参数编码
string queryString = UrlCrypto.EncodeParameters(new Dictionary<string, string>
{
    ["key1"] = "value1",
    ["key2"] = "value2"
});

// 解析 URL 参数
Dictionary<string, string> parameters = UrlCrypto.DecodeParameters("key1=value1&key2=value2");
```

## 完整示例

### 敏感数据存储（推荐使用AES-GCM）

```csharp
using Materal.Utils.Crypto;

namespace Example
{
    public class SecureDataService(ICryptoHelper cryptoHelper)
    {
        private readonly ICryptoHelper _cryptoHelper = cryptoHelper;

        /// <summary>
        /// 加密敏感数据
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns>加密后的数据（包含密钥）</returns>
        public string EncryptData(string data)
        {
            return _cryptoHelper.Encrypt(data);
        }

        /// <summary>
        /// 解密敏感数据
        /// </summary>
        /// <param name="encryptedData">加密数据</param>
        /// <returns>解密后的数据</returns>
        public async Task<string> DecryptDataAsync(string encryptedData)
        {
            return _cryptoHelper.Decrypt(encryptedData);
        }
    }
}
```

### 大文件传输（RSA+AES混合>原始数据加密）

```csharp
using Materal.Utils.Crypto;

namespace Example
{
    public class FileEncryptionService(ICryptoHelper cryptoHelper)
    {
        private readonly ICryptoHelper _cryptoHelper = cryptoHelper;

        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="rsaPublicKey">RSA公钥</param>
        /// <returns>加密后的文件数据</returns>
        public async Task<byte[]> EncryptFileAsync(string filePath, string rsaPublicKey)
        {
            using FileStream inputStream = File.OpenRead(filePath);
            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream);
            return HybridCrypto.Encrypt(memoryStream.ToArray(), rsaPublicKey);
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="encryptedData">加密数据</param>
        /// <param name="rsaPrivateKey">RSA私钥</param>
        /// <returns>解密后的文件数据</returns>
        public async Task<byte[]> DecryptFileAsync(byte[] encryptedData, string rsaPrivateKey)
        {
            return await Task.FromResult(HybridCrypto.Decrypt(encryptedData, rsaPrivateKey));
        }
    }
}
```

### 数字签名验证

```csharp
using Materal.Utils.Crypto;

namespace Example
{
    public class SignatureService(IRsaCrypto rsaCrypto)
    {
        private readonly IRsaCrypto _rsaCrypto = rsaCrypto;

        /// <summary>
        /// 生成密钥对
        /// </summary>
        /// <param name="keySize">密钥大小（位数）</param>
        /// <returns>公钥和私钥元组</returns>
        public (string publicKey, string privateKey) GenerateKeyPair(int keySize = 2048)
        {
            return _rsaCrypto.GenerateKeyPair(keySize);
        }

        /// <summary>
        /// 签名数据
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="privateKey">RSA私钥</param>
        /// <returns>数字签名</returns>
        public string SignData(string data, string privateKey)
        {
            return _rsaCrypto.SignText(data, privateKey);
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="signature">数字签名</param>
        /// <param name="publicKey">RSA公钥</param>
        /// <returns>签名是否有效</returns>
        public bool VerifySignature(string data, string signature, string publicKey)
        {
            return _rsaCrypto.VerifyText(data, signature, publicKey);
        }
    }
}
```

## 安全注意事项

| 加密方式 | 安全等级 | 使用建议 |
|----------|----------|----------|
| MD5 | 不安全 | 仅用于文件校验和等非安全场景 |
| SHA256 | 安全 | 推荐用于哈希场景 |
| AES-CBC | 安全 | 每次加密需使用随机 IV |
| AES-GCM | 高安全 | 推荐用于敏感数据，提供认证加密 |
| RSA | 安全 | 建议使用 2048 位或更长密钥 |
| 混合加密 | 高安全 | 推荐用于大数据量传输场景 |

## KeyFormat 枚举

```csharp
public enum KeyFormat
{
    /// <summary>
    /// XML格式
    /// </summary>
    Xml,

    /// <summary>
    /// PEM公钥格式
    /// </summary>
    PemPublic,

    /// <summary>
    /// PEM私钥格式
    /// </summary>
    PemPrivate
}
```
