# Perception & Entropie

Projet réalisé dans le cadre du module **IEVA** (Master SIIA, ENIB / UBO).

---

## 1. Comment marche le jeu

**Perception & Entropie** est un jeu d'arène compétitif local à 2 joueurs sur un même clavier. Le jeu s'inspire du thème *« We don't see things as they are, we see them as we are »* : les couleurs de l'arène évoluent selon le joueur qui domine la partie, avec une palette visuelle choisie aléatoirement au lancement.

### Déroulement d'une partie
- **Objectif :** Être le premier joueur à atteindre **100 points**.
- **Capturer les zones :** Il y a 3 zones mouvantes dans l'arène. Il faut rester seul dans une zone pendant **2.8 secondes** pour la capturer. Une fois capturée, elle rapporte des points régulièrement (+4 points toutes les 2s), même si on la quitte.
- **Zones dynamiques :** Toutes les **30 secondes**, les zones changent de position sur la carte et redeviennent neutres.
- **Bousculades (Sumo) :** Le dash permet de projeter l'adversaire dans les airs hors des zones pour contester le contrôle.
- **Survie & Entropie :** En dehors des zones, on perd progressivement de la vie (entropie). Rester dans une zone permet de se régénérer. On peut aussi récupérer des Totems de Soin (+40 HP). Attention aux Gardiens qui patrouillent et attaquent les joueurs hors des zones sûres !

---

## 2. Commandes

| Action | Joueur 1 (Gauche) | Joueur 2 (Droite) |
| :--- | :--- | :--- |
| **Déplacement** | **ZQSD** (AZERTY) ou **WASD** | **Flèches directionnelles** |
| **Dash (Bousculade)** | **Espace** | **Shift Droit** (ou Entrée / Pavé 0) |

- **Échap :** Menu Pause  
- **R :** Recommencer la partie rapidement  

---

## 3. Répartition du travail

La répartition s'appuie sur le découpage convenu pour le projet :

### Dev 1 — Bilal
- Contrôles et déplacements physiques des joueurs
- Caméra partagée suivant les deux joueurs
- Mécaniques de bump et de dash avec cooldown
- Interface utilisateur : HUD et écran de fin
- Menu principal
- Feedbacks d'impact et secousses de caméra

### Dev 2 — Paulo
- Système de capture des zones
- Jauge de cohérence, entropie et survie hors zone
- GameManager, comptage des points et condition de victoire
- IA des Gardiens avec navigation NavMesh
- Plaques de pression et portes dynamiques
- Effets sonores et ambiance audio
- Système de génération de palettes aléatoires
- Intégration de la scène finale

### Travail commun
- Conception et assemblage initial de l'arène
- Direction artistique, shaders et rendu visuel
- Équilibrage global du gameplay, tests et vérifications du jeu
