# ZRScanner

Application Android .NET MAUI pour scanner des documents, les conserver en PDF sur l’appareil et les partager avec les applications installées (mail, messagerie, cloud, etc.).

## Fonctions

- Scanner des pages avec le scanner Android intégré au plugin `Plugin.Maui.DocumentScanner`.
- Créer et conserver les PDF dans l’espace privé de l’application.
- Afficher la liste des documents, ouvrir un PDF ou le partager via le menu de partage Android.

Les fichiers restent locaux jusqu’à ce que l’utilisateur choisisse une application de partage. Aucun compte ou service cloud n’est nécessaire.

## Architecture

- `src/ZRScanner.Domain` : entités et règles métier.
- `src/ZRScanner.Application` : cas d’usage et ports.
- `src/ZRScanner.Infrastructure` : stockage local et génération PDF.
- `src/ZRScanner.Mobile` : interface .NET MAUI et intégration Android.

## Build Android

Le workflow GitHub Actions compile et publie un APK sur chaque push ou pull request vers `main` (artefact conservé 14 jours). Si les secrets de signature sont configurés, l’APK Release est signé avec la clé stable ; sinon, le workflow produit un APK Debug signé pour les tests.

Pour activer les builds Release signés, configurer ces secrets dans le dépôt GitHub :

- `ANDROID_SIGNING_KEY_BASE64` : keystore JKS encodé en Base64.
- `ANDROID_SIGNING_PASSWORD` : mot de passe du keystore et de la clé.

L’alias du keystore utilisé par le workflow est `tcfconsult`.

L’application utilise l’identifiant Android `com.benbelgacemzied.zrscanner`.
