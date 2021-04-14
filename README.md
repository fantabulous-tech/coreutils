# CoreUtils

Structural systems and tools that make using Unity easier and more productive.

## Installation

### Option #1: OpenUPM

Use the OpenUPM command line interface:
- `openupm add tech.fantabulous.coreutils`

Add in Project Settings:

- Go to `Edit > Project Settings > Package manager`
- Click the `+` button in the registry list
- Name it `OpenUPM`
- Set the URL to `https://package.openupm.com`
- Click the `+` butotn in the scope list
- Add `tech.fantabulous.coreutils`
- Open `Package Manager > My Registeries`
- Add `CoreUtils` package.

### Option #2: Package Manager Window

To install from Unity v.2019.4+:
- Make sure you have [git](https://git-scm.com/downloads) installed and in your [PATH variable](https://www.google.com/search?q=windows+10+environment+variables+window)
- Open the Package Manager
- Click the `+` button
- Select 'Add Package from Git URL'
- Paste in: `https://github.com/fantabulous-tech/coreutils.git?path=/Packages/CoreUtils`

### Option #3: Edit `manifest.json`

Alternatively, you can manually change your `<Project>/Packages/manifest.json`:
```json
{
  "dependencies": {
    "tech.fantabulous.coreutils": "https://github.com/fantabulous-tech/coreutils.git?path=/Packages/CoreUtils"
  }
}
```

### Option #4: Manual Copy of CoreUtils Folder

And finally, if those don't work or you don't want to install git, you can manually copy `Packages/CoreUtils` folder into your own `<Project>/Packages/` folder.

## CoreUtils Documentation

See the [CoreUtils Unity Package Doc](https://github.com/fantabulous-tech/coreutils/blob/master/Packages/CoreUtils/Documentation~/CoreUtils.md) for more info about the individual tools and systems.

## License

MIT License

Copyright (c) 2021 Fantabulous Tech

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
