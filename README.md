<p align="center">
  <a href="https://paypal.me/gavazquez"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg?style=flat&logo=paypal" alt="PayPal"/></a>
  <a href="https://discord.gg/S6bQR5q"><img src="https://img.shields.io/discord/378456662392045571.svg?style=flat&logo=discord&label=discord" alt="Chat on discord"/></a>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/CachedQuickLz"><img src="https://img.shields.io/nuget/v/CachedQuickLz.svg?style=flat&logo=nuget" alt="Nuget" /></a>
  <a href="../../releases"><img src="https://img.shields.io/github/release/lunamultiplayer/cachedquicklz.svg?style=flat&logo=github&logoColor=white" alt="Latest release" /></a>
  <a href="../../releases"><img src="https://img.shields.io/github/downloads/lunamultiplayer/cachedquicklz/total.svg?style=flat&logo=github&logoColor=white" alt="Total downloads" /></a>
  <a href="../../"><img src="https://img.shields.io/github/search/lunamultiplayer/cachedquicklz/goto.svg?style=flat&logo=github&logoColor=white" alt="Total hits" /></a>
</p>

---

# Cached compression/decompression with [QuickLz](http://www.quicklz.com/)

*Allows you to compress and decompress with [QuickLz](http://www.quicklz.com/) while keeping low the GC*  

### Main features:

- Fast compression/decompression by using the [QuickLz](http://www.quicklz.com/) algorithm
- Uses an array pool to reuse the arrays

### Usage:

##### Compress:
```CSharp
var length = 5000;
var data = new byte[length]
//Fill up the "data" array...
CachedQlz.Compress(ref data, ref length);
//Now "data" is an array with the compressed bytes. If you want to check it's length use the variable "length"
//Do NOT use data.Length! It might be bigger as it came from a cached array!
```

##### Decompress:
```CSharp
//"data" is an array with compressed bytes...
CachedQlz.Decompress(ref data, out var decompressedLength);
//Now "data" is an array with the decompressed bytes. If you want to check it's length use the variable "length"
//Do NOT use data.Length! It might be bigger as it came from a cached array!
```

##### Request an array from the pool:
```CSharp
var array = ArrayPool<byte>.Spawn(4);
//Caution, array.Length will 8 instead of 4 as the cache system uses values based on powers of two
```

##### Return an array to the pool:
```CSharp
ArrayPool<byte>.Recycle(array);
```

---

### Status:

|   Branch   |   Build  |   Tests  |
| ---------- | -------- | -------- |
| **master** |[![AppVeyor](https://img.shields.io/appveyor/ci/gavazquez/cachedquicklz/master.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/cachedquicklz/branch/master) | [![AppVeyor Tests](https://img.shields.io/appveyor/tests/gavazquez/cachedquicklz/master.svg?logo=appveyor)](https://ci.appveyor.com/project/gavazquez/cachedquicklz/branch/master/tests)

---

<p align="center">
  <a href="mailto:gavazquez@gmail.com"><img src="https://img.shields.io/badge/email-gavazquez@gmail.com-blue.svg?style=flat" alt="Email: gavazquez@gmail.com" /></a>
  <a href="./LICENSE"><img src="https://img.shields.io/github/license/lunamultiplayer/cachedquicklz.svg" alt="License" /></a>
</p>
