# CoreUtils

Structural systems and tools that make using Unity easier and more productive.

## Installation

### Option #1: Package Manager Window

To install from Unity v.2019.4+:
- Make sure you have [git](https://git-scm.com/downloads) installed and in your [PATH variable](https://www.google.com/search?q=windows+10+environment+variables+window)
- Open the Package Manager
- Click the `+` button
- Select 'Add Package from Git URL'
- Paste in: `https://github.com/fantabulous-tech/coreutils.git?path=/Packages/CoreUtils`

### Option #2: Edit `manifest.json`

Alternatively, you can manually change your `<Project>/Packages/manifest.json`:
```
{
  "dependencies": {
    "tech.fantabulous.coreutils": "https://github.com/fantabulous-tech/coreutils.git?path=/Packages/CoreUtils"
  }
}
```

### Option #3: Manual Copy of CoreUtils Folder

And finally, if those don't work or you don't want to install git, you can manually copy `Packages/CoreUtils` folder into your own `<Project>/Packages/` folder.

## CoreUtils Documentation

See the [CoreUtils Unity Package Doc](https://github.com/fantabulous-tech/coreutils/blob/master/Packages/CoreUtils/Documentation~/CoreUtils.md) for more info about the individual tools and systems.

## License

[MIT](https://github.com/fantabulous-tech/coreutils/blob/master/Packages/CoreUtils/LICENSE.md)
