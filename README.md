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

Le workflow GitHub Actions produit un APK et un AAB signés sur chaque push vers `main`, ou à la demande depuis l’onglet **Actions**. Les artefacts `ZRScanner-Android-APK` et `ZRScanner-Android-AAB` sont conservés 14 jours.

Configurer ces quatre secrets dans **Settings > Secrets and variables > Actions** du dépôt. Ils sont utilisés pour signer l’APK de test et l’AAB à envoyer sur Google Play :

- `ANDROID_SIGNING_KEY_BASE64` : fichier JKS encodé en Base64.
- `ANDROID_SIGNING_STORE_PASSWORD` : mot de passe du keystore.
- `ANDROID_SIGNING_KEY_ALIAS` : alias exact de la clé dans le keystore.
- `ANDROID_SIGNING_KEY_PASSWORD` : mot de passe de la clé.

Utiliser une clé dédiée à `ZRScanner`, conserver une sauvegarde sécurisée du keystore et ne jamais ajouter ses valeurs au dépôt.

L’application utilise l’identifiant Android `com.benbelgacemzied.zrscanner`.
