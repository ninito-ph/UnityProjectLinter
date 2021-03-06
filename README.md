<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/ninito-ph/UnityProjectLinter">
    <img src="https://i.imgur.com/RS1Tymp.png" alt="Unity Project Linter" width="549" height="314">
    <p align="center">
    <img alt="GitHub tag (latest SemVer)" src="https://img.shields.io/github/v/tag/ninito-ph/UnityProjectLinter?label=version&style=for-the-badge">
    <img alt="GitHub repo size" src="https://img.shields.io/github/repo-size/ninito-ph/UnityProjectLinter?label=size&style=for-the-badge">
    <img alt="GitHub Repo stars" src="https://img.shields.io/github/stars/ninito-ph/UnityProjectLinter?style=for-the-badge">
    </p>
  </a>


<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

Unity Project Linter is a simple tool that allows you to ensure asset naming conventions are being followed in your project.
  
<img src="https://i.imgur.com/geAd2fT.gif" alt="Unity Project Linter" width="774" height="410">

Here are its features:
* Ensure assets names have the necessary prefixes, infixes and suffixes.
* Ensure asset names don't have certain characters, such as spaces.
* Customize existing rules easily.
* Create new naming rules, even for complex cases, by extending the simple NamingRule class.
* Handle multiple competing rules by assigning them differing priorities.
* Ignore individual assets or entire folders from naming rules.
* Receive console warnings when an asset is renamed or created and does not match naming rules.
* Export a log of assets that violate naming rules.
* Create custom log exporters for asset naming violation, such as text loggers and csv loggers.
* Mass-rename assets that violate rules.



<!-- GETTING STARTED -->
## Getting Started

Import the package into your project, and then use the create menu to create an Asset Linting Settings asset.

Create > Unity Project Linter > Asset Linting Settings


### Installation

1. Open the package manager.
2. Click on the plus icon on the top left corner, and select 'Add package from git URL'.
3. Paste in '[https://github.com/ninito-ph/UnityProjectLinter.git](https://github.com/ninito-ph/UnityProjectLinter.git)'.
4. Import it into your project!


<!-- CONTACT -->
## Contact

Paulo Oliveira - paulo at ninito dot me

Project Link: [https://github.com/ninito-ph/UnityProjectLinter](https://github.com/ninito-ph/UnityProjectLinter)


