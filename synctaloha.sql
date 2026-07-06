-- ==========================================================================
-- SCRIPT DE SYNCHRONISATION INTEGRALE OPTIMISÉE V3.8
-- CONFIGURATION : ERIC GERALDIN  (LAST WRITE WINS)
-- ==========================================================================

-- Liaison temporaire avec la base de données téléchargée depuis le Cloud
ATTACH DATABASE 'C:/Users/HP/Downloads/PERSLRRSANSCODE.db' AS internet;


-- ==========================================================================
-- ÉTAPE 1 : SYNCHRONISATION DES DECHARGES (PRIORITÉ ABSOLUE)
-- ==========================================================================
-- NOTE : Les décharges sont des transactions historiques immuables. 
-- Pas besoin de "Last Write", on se contente de combler les trous des deux côtés.

-- A. Réception : On récupère sur le PC les nouvelles décharges saisies sur le Cloud
INSERT INTO main.DECHARGE (PersonnelNom, ArticleNom, Quantite, Unite, DateDecharge, SignaturePath)
SELECT PersonnelNom, ArticleNom, Quantite, Unite, DateDecharge, SignaturePath FROM internet.DECHARGE
WHERE NOT EXISTS (
    SELECT 1 FROM main.DECHARGE 
    WHERE main.DECHARGE.PersonnelNom = internet.DECHARGE.PersonnelNom
      AND main.DECHARGE.ArticleNom = internet.DECHARGE.ArticleNom
      AND main.DECHARGE.DateDecharge = internet.DECHARGE.DateDecharge
);

-- B. Envoi : On envoie sur le Cloud les nouvelles décharges saisies en local sur le PC
INSERT INTO internet.DECHARGE (PersonnelNom, ArticleNom, Quantite, Unite, DateDecharge, SignaturePath)
SELECT PersonnelNom, ArticleNom, Quantite, Unite, DateDecharge, SignaturePath FROM main.DECHARGE
WHERE NOT EXISTS (
    SELECT 1 FROM internet.DECHARGE 
    WHERE internet.DECHARGE.PersonnelNom = main.DECHARGE.PersonnelNom
      AND internet.DECHARGE.ArticleNom = main.DECHARGE.ArticleNom
      AND internet.DECHARGE.DateDecharge = main.DECHARGE.DateDecharge
);


-- ==========================================================================
-- ÉTAPE 2 : ALIGNEMENT DES ARTICLES ET DES STOCKS (LAST WRITE WINS)
-- ==========================================================================

-- A1. PUSH (PC -> Cloud) : On met à jour le Cloud si le PC a une modif plus récente
UPDATE internet.ARTICLE
SET 
    Quantite = a.Quantite,
    Unite = a.Unite,
    StockSec = a.StockSec,
    DateModif = a.DateModif
FROM main.ARTICLE a
WHERE internet.ARTICLE.Nom = a.Nom
  AND (internet.ARTICLE.DateModif IS NULL OR a.DateModif > internet.ARTICLE.DateModif);

-- A2. PULL (Cloud -> PC) : On met à jour le PC si le Cloud a une modif plus récente
UPDATE main.ARTICLE
SET 
    Quantite = i.Quantite,
    Unite = i.Unite,
    StockSec = i.StockSec,
    DateModif = i.DateModif
FROM internet.ARTICLE i
WHERE main.ARTICLE.Nom = i.Nom
  AND (main.ARTICLE.DateModif IS NULL OR i.DateModif > main.ARTICLE.DateModif);

-- B. Insertion vers Cloud : Si un nouvel article a été créé sur le PC
INSERT INTO internet.ARTICLE (Nom, Quantite, Unite, StockSec, DateModif)
SELECT Nom, Quantite, Unite, StockSec, DateModif FROM main.ARTICLE
WHERE NOT EXISTS (SELECT 1 FROM internet.ARTICLE WHERE internet.ARTICLE.Nom = main.ARTICLE.Nom);

-- C. Insertion vers PC : Si un nouvel article a été créé sur le Cloud
INSERT INTO main.ARTICLE (Nom, Quantite, Unite, StockSec, DateModif)
SELECT Nom, Quantite, Unite, StockSec, DateModif FROM internet.ARTICLE
WHERE NOT EXISTS (SELECT 1 FROM main.ARTICLE WHERE main.ARTICLE.Nom = internet.ARTICLE.Nom);


-- ==========================================================================
-- ÉTAPE 3 : FICHES DU PERSONNEL (TABLE BASE - LAST WRITE WINS)
-- ==========================================================================

-- A1. PUSH (PC -> Cloud) : On pousse vers le Cloud si la fiche PC est plus récente
UPDATE internet.BASE
SET 
    MATRICULE = b.MATRICULE, CIN = b.CIN, DEC = b.DEC, CORPS = b.CORPS, MATIERE = b.MATIERE, 
    DATENAISS = b.DATENAISS, LIEUDENAISS = b.LIEUDENAISS, SEXE = b.SEXE, STATUT = b.STATUT, 
    DATEDENTRE = b.DATEDENTRE, DATEDEPRISE = b.DATEDEPRISE, DIPLOMEAC = b.DIPLOMEAC, 
    DIPLOMEPED = b.DIPLOMEPED, CONTACT = b.CONTACT, FONCTION = b.FONCTION, Photo = b.Photo, 
    PERAV = b.PERAV, DEMAV = b.DEMAV, TEMAV = b.TEMAV, QEMAV = b.QEMAV, CEMAV = b.CEMAV, 
    SEMAV = b.SEMAV, SEPMAV = b.SEPMAV, HEMAV = b.HEMAV, NEMAV = b.NEMAV, DXEMAV = b.DXEMAV, 
    ONEMAV = b.ONEMAV, DOU = b.DOU, TREI = b.TREI, QUAT = b.QUAT, QUIN = b.QUIN, SEIZ = b.SEIZ,
    GRADE = b.GRADE, SERIE_BACC = b.SERIE_BACC,
    CLASSE_TENUE_1 = b.CLASSE_TENUE_1, CLASSE_TENUE_2 = b.CLASSE_TENUE_2, 
    CLASSE_TENUE_3 = b.CLASSE_TENUE_3, CLASSE_TENUE_4 = b.CLASSE_TENUE_4, 
    CLASSE_TENUE_5 = b.CLASSE_TENUE_5, CLASSE_TENUE_6 = b.CLASSE_TENUE_6, 
    CLASSE_TENUE_7 = b.CLASSE_TENUE_7, CLASSE_TENUE_8 = b.CLASSE_TENUE_8,
    DateModif = b.DateModif
FROM main.BASE b 
WHERE internet.BASE.NOM_ET_PRENOMS = b.NOM_ET_PRENOMS
  AND (internet.BASE.DateModif IS NULL OR b.DateModif > internet.BASE.DateModif);

-- A2. PULL (Cloud -> PC) : On rapatrie sur le PC si la fiche Cloud est plus récente
UPDATE main.BASE
SET 
    MATRICULE = i.MATRICULE, CIN = i.CIN, DEC = i.DEC, CORPS = i.CORPS, MATIERE = i.MATIERE, 
    DATENAISS = i.DATENAISS, LIEUDENAISS = i.LIEUDENAISS, SEXE = i.SEXE, STATUT = i.STATUT, 
    DATEDENTRE = i.DATEDENTRE, DATEDEPRISE = i.DATEDEPRISE, DIPLOMEAC = i.DIPLOMEAC, 
    DIPLOMEPED = i.DIPLOMEPED, CONTACT = i.CONTACT, FONCTION = i.FONCTION, Photo = i.Photo, 
    PERAV = i.PERAV, DEMAV = i.DEMAV, TEMAV = i.TEMAV, QEMAV = i.QEMAV, CEMAV = i.CEMAV, 
    SEMAV = i.SEMAV, SEPMAV = i.SEPMAV, HEMAV = i.HEMAV, NEMAV = i.NEMAV, DXEMAV = i.DXEMAV, 
    ONEMAV = i.ONEMAV, DOU = i.DOU, TREI = i.TREI, QUAT = i.QUAT, QUIN = i.QUIN, SEIZ = i.SEIZ,
    GRADE = i.GRADE, SERIE_BACC = i.SERIE_BACC,
    CLASSE_TENUE_1 = i.CLASSE_TENUE_1, CLASSE_TENUE_2 = i.CLASSE_TENUE_2, 
    CLASSE_TENUE_3 = i.CLASSE_TENUE_3, CLASSE_TENUE_4 = i.CLASSE_TENUE_4, 
    CLASSE_TENUE_5 = i.CLASSE_TENUE_5, CLASSE_TENUE_6 = i.CLASSE_TENUE_6, 
    CLASSE_TENUE_7 = i.CLASSE_TENUE_7, CLASSE_TENUE_8 = i.CLASSE_TENUE_8,
    DateModif = i.DateModif
FROM internet.BASE i 
WHERE main.BASE.NOM_ET_PRENOMS = i.NOM_ET_PRENOMS
  AND (main.BASE.DateModif IS NULL OR i.DateModif > main.BASE.DateModif);

-- B. Insertion vers PC : On ajoute sur le PC les nouvelles personnes créées depuis le Cloud
INSERT INTO main.BASE (
    MATRICULE, NOM_ET_PRENOMS, CIN, DEC, CORPS, MATIERE, DATENAISS, LIEUDENAISS, SEXE, STATUT, 
    DATEDENTRE, DATEDEPRISE, DIPLOMEAC, DIPLOMEPED, CONTACT, FONCTION, Photo, PERAV, DEMAV, 
    TEMAV, QEMAV, CEMAV, SEMAV, SEPMAV, HEMAV, NEMAV, DXEMAV, ONEMAV, DOU, TREI, QUAT, QUIN, SEIZ, 
    GRADE, SERIE_BACC, CLASSE_TENUE_1, CLASSE_TENUE_2, CLASSE_TENUE_3, CLASSE_TENUE_4, 
    CLASSE_TENUE_5, CLASSE_TENUE_6, CLASSE_TENUE_7, CLASSE_TENUE_8, DateModif
)
SELECT 
    MATRICULE, NOM_ET_PRENOMS, CIN, DEC, CORPS, MATIERE, DATENAISS, LIEUDENAISS, SEXE, STATUT, 
    DATEDENTRE, DATEDEPRISE, DIPLOMEAC, DIPLOMEPED, CONTACT, FONCTION, Photo, PERAV, DEMAV, 
    TEMAV, QEMAV, CEMAV, SEMAV, SEPMAV, HEMAV, NEMAV, DXEMAV, ONEMAV, DOU, TREI, QUAT, QUIN, SEIZ, 
    GRADE, SERIE_BACC, CLASSE_TENUE_1, CLASSE_TENUE_2, CLASSE_TENUE_3, CLASSE_TENUE_4, 
    CLASSE_TENUE_5, CLASSE_TENUE_6, CLASSE_TENUE_7, CLASSE_TENUE_8, DateModif 
FROM internet.BASE
WHERE NOT EXISTS (SELECT 1 FROM main.BASE WHERE main.BASE.NOM_ET_PRENOMS = internet.BASE.NOM_ET_PRENOMS);

-- C. Insertion vers Cloud : On ajoute sur le Cloud les nouvelles personnes créées localement sur le PC
INSERT INTO internet.BASE (
    MATRICULE, NOM_ET_PRENOMS, CIN, DEC, CORPS, MATIERE, DATENAISS, LIEUDENAISS, SEXE, STATUT, 
    DATEDENTRE, DATEDEPRISE, DIPLOMEAC, DIPLOMEPED, CONTACT, FONCTION, Photo, PERAV, DEMAV, 
    TEMAV, QEMAV, CEMAV, SEMAV, SEPMAV, HEMAV, NEMAV, DXEMAV, ONEMAV, DOU, TREI, QUAT, QUIN, SEIZ, 
    GRADE, SERIE_BACC, CLASSE_TENUE_1, CLASSE_TENUE_2, CLASSE_TENUE_3, CLASSE_TENUE_4, 
    CLASSE_TENUE_5, CLASSE_TENUE_6, CLASSE_TENUE_7, CLASSE_TENUE_8, DateModif
)
SELECT 
    MATRICULE, NOM_ET_PRENOMS, CIN, DEC, CORPS, MATIERE, DATENAISS, LIEUDENAISS, SEXE, STATUT, 
    DATEDENTRE, DATEDEPRISE, DIPLOMEAC, DIPLOMEPED, CONTACT, FONCTION, Photo, PERAV, DEMAV, 
    TEMAV, QEMAV, CEMAV, SEMAV, SEPMAV, HEMAV, NEMAV, DXEMAV, ONEMAV, DOU, TREI, QUAT, QUIN, SEIZ, 
    GRADE, SERIE_BACC, CLASSE_TENUE_1, CLASSE_TENUE_2, CLASSE_TENUE_3, CLASSE_TENUE_4, 
    CLASSE_TENUE_5, CLASSE_TENUE_6, CLASSE_TENUE_7, CLASSE_TENUE_8, DateModif 
FROM main.BASE
WHERE NOT EXISTS (SELECT 1 FROM internet.BASE WHERE internet.BASE.NOM_ET_PRENOMS = main.BASE.NOM_ET_PRENOMS);


-- ==========================================================================
-- FIN DE LA SÉQUENCE SÉCURISÉE
-- ==========================================================================
DETACH DATABASE internet;