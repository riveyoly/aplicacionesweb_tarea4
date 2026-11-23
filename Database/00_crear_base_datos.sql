-- =====================================================================
--  SISTEMA DE GESTION DE TICKETS - Creacion de la base de datos
--  Solo para PostgreSQL instalado en el equipo (pgAdmin), conectado a la
--  base "postgres". Con Docker NO hace falta: el contenedor la crea solo.
-- =====================================================================

CREATE DATABASE gestion_tickets
    WITH ENCODING = 'UTF8'
         TEMPLATE = template0;
