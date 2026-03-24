# Installation

Cette page couvre l'installation de DocTools et de ses dépendances.

## Prérequis

| Prérequis | Version | Comment vérifier |
|-----------|---------|-----------------|
| **Bash** | Toute version (Git Bash sous Windows) | `bash --version` |
| **SDK .NET** | 9.0+ | `dotnet --version` |
| **DocFX** | Dernière version | `docfx --version` |

## Installer le SDK .NET

Téléchargez et installez le [SDK .NET](https://dotnet.microsoft.com/download) (version 9.0 ou ultérieure). DocFX en a besoin pour l'extraction des métadonnées C#.

## Installer DocFX

Installez DocFX en tant qu'outil global .NET :

```bash
dotnet tool install -g docfx
```

Vérifiez qu'il est dans votre PATH :

```bash
docfx --version
```

## Installer DocTools

Clonez le dépôt DocTools :

```bash
git clone <url-du-depot-doctools> /chemin/vers/DocTools
```

Aucune installation supplémentaire n'est nécessaire. Toutes les commandes s'exécutent directement via `doctools.sh`.

> [!TIP]
> Ajoutez le répertoire DocTools à votre `PATH` pour pouvoir appeler `doctools.sh` depuis n'importe où :
> ```bash
> export PATH="$PATH:/chemin/vers/DocTools"
> ```

## Vérifier l'installation

Exécutez `doctools.sh` sans arguments pour voir le message d'utilisation :

```bash
doctools.sh
```

Vous devriez voir la liste des commandes disponibles (init, build, translate, etc.).

## Étape suivante

Passez au [Démarrage rapide](quickstart.md) pour initialiser la documentation d'un package et construire votre premier site.
