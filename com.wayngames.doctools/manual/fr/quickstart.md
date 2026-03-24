# Demarrage rapide

Ce guide vous permet de passer de zero a un site de documentation genere en moins de 5 minutes.

**Objectif** : Initialiser la documentation d'un package Unity et visualiser le site genere dans votre navigateur.

## Prerequis

- DocTools installe ([Installation](install.md))
- Un package Unity avec un fichier `package.json` contenant les champs `name`, `displayName` et `version`

## Etape 1 : Initialiser la documentation

Executez `init` en pointant vers le repertoire de votre package :

```bash
doctools.sh init ./MyPackage
```

Cela cree un repertoire `Documentation/` avec des fichiers de base :

```
MyPackage/
├── package.json
└── Documentation/
    ├── toc.md              # Table des matieres laterale
    ├── index.md            # Page d'accueil
    ├── getting-started.md  # Guide de demarrage
    └── Images/
        └── logo.png        # Logo par defaut
```

Le titre est lu automatiquement depuis le champ `displayName` du fichier `package.json`. Pour le remplacer :

```bash
doctools.sh init ./MyPackage --title "My Custom Title"
```

## Etape 2 : Ajouter une page

Ouvrez `Documentation/getting-started.md` dans votre editeur et ajoutez du contenu. Le fichier genere contient deja une structure de base que vous pouvez personnaliser.

Pour ajouter une nouvelle page :

1. Creez un nouveau fichier `.md` dans `Documentation/` (par exemple, `architecture.md`)
2. Ajoutez un lien vers celui-ci dans `Documentation/toc.md` :
   ```markdown
   # [Architecture](architecture.md)
   ```

## Etape 3 : Generer le site

Executez la commande de generation :

```bash
doctools.sh build ./MyPackage
```

Le resultat est place dans `MyPackage/Documentation~/` :

```
MyPackage/
└── Documentation~/
    ├── index.html      # Redirection vers la version courante
    ├── versions.js     # Donnees du selecteur de version
    └── v1.0.0/
        ├── index.html  # Page d'accueil
        ├── manual/     # Vos pages markdown (HTML)
        ├── api/        # Reference API C# (generee automatiquement)
        └── styles/     # CSS et JavaScript du theme
```

## Etape 4 : Visualiser le site

Ouvrez le site genere dans votre navigateur :

```bash
# Windows (Git Bash)
start MyPackage/Documentation~/v1.0.0/index.html

# macOS
open MyPackage/Documentation~/v1.0.0/index.html

# Linux
xdg-open MyPackage/Documentation~/v1.0.0/index.html
```

Vous devriez voir un site de documentation avec :

- Une **barre de navigation** avec le titre de votre package, une barre de recherche et un bouton de basculement mode sombre
- Une **barre laterale** avec votre table des matieres
- Votre **contenu** rendu a partir du markdown
- Une section **API** avec la documentation de reference C# generee automatiquement

## Que s'est-il passe ?

La commande `build` a execute un pipeline :

1. **Extraction des metadonnees** -- DocFX a analyse vos fichiers source C# et genere la documentation API (fichiers YAML)
2. **Traitement du contenu** -- DocFX a converti vos pages markdown et les fichiers YAML API en HTML
3. **Application du modele** -- Le modele WaynGames.StaticToc.Extension a ajoute le mode sombre/clair, le selecteur de version, la recherche et le style
4. **Generation de la sortie** -- `versions.js` et une redirection racine ont ete crees pour le selecteur de version

Le numero de version (`v1.0.0`) provient du champ `version` de votre `package.json`. Lorsque vous incrementez la version et reconstruisez, les deux versions apparaissent dans le menu deroulant des versions.

## Etapes suivantes

- [Configuration](configuration.md) -- Comprendre comment DocTools lit les metadonnees de votre package
- [Versionnage](versioning.md) -- Generer la documentation pour plusieurs versions
- [Traduction](translation.md) -- Ajouter le support multilingue
- [Commandes](commands.md) -- Reference complete de la CLI
