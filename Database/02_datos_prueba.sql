-- =====================================================================
--  SISTEMA DE GESTION DE TICKETS - Datos de ejemplo
--  Ejecutar despues de 01_crear_tablas.sql (opcional).
-- =====================================================================

INSERT INTO categorias (categoria_id, nombre, descripcion, prioridad, tiempo_respuesta_horas, activa) VALUES
 (1, 'Hardware',         'Computadoras, impresoras y periféricos',  'Media',   24, true),
 (2, 'Software',         'Instalación y errores de programas',      'Media',   48, true),
 (3, 'Redes',            'Internet, Wi-Fi y VPN',                   'Alta',     8, true),
 (4, 'Correo y cuentas', 'Contraseñas, accesos y correo',           'Alta',     4, true),
 (5, 'Seguridad',        'Virus, phishing y accesos indebidos',     'Crítica',  2, true),
 (6, 'Telefonía',        'Extensiones y teléfonos IP',              'Baja',    72, false);

INSERT INTO usuarios (usuario_id, cedula, nombre, correo, telefono, departamento) VALUES
 (1, '1712345678', 'Yolanda Rivera',  'yolanda.rivera@empresa.com',  '0991234567', 'Gerencia'),
 (2, '0923456781', 'Carlos Mendoza',  'carlos.mendoza@empresa.com',  '0987654321', 'Contabilidad'),
 (3, '1103224563', 'Silvia Suárez',   'silvia.suarez@empresa.com',   '0976543210', 'Ventas'),
 (4, '1309876543', 'Andrés Paredes',  'andres.paredes@empresa.com',  '0965432109', 'Recursos Humanos');

INSERT INTO tecnicos (tecnico_id, nombre, correo, telefono, especialidad, activo) VALUES
 (1, 'Luis Torres',    'luis.torres@soporte.com',    '0981112233', 'Redes y seguridad',      true),
 (2, 'María Cevallos', 'maria.cevallos@soporte.com', '0982223344', 'Software y cuentas',     true),
 (3, 'Jorge Andrade',  'jorge.andrade@soporte.com',  '0983334455', 'Hardware y equipos',     true),
 (4, 'Paola Rivas',    'paola.rivas@soporte.com',    '0984445566', 'Telefonía',              false);

INSERT INTO tickets (ticket_id, asunto, descripcion, fecha_apertura, usuario_id, categoria_id) VALUES
 (1, 'La impresora del piso 2 no enciende', 'La impresora no enciende desde esta mañana.',              '2026-11-16', 1, 1),
 (2, 'Olvidé la contraseña del correo',     'No puedo ingresar al correo institucional.',              '2026-11-18', 1, 4),
 (3, 'Internet lento en Contabilidad',      'Las páginas tardan más de un minuto en cargar.',          '2026-11-19', 2, 3),
 (4, 'Instalar el sistema contable',        'Se necesita el sistema contable en el equipo nuevo.',     '2026-11-20', 3, 2),
 (5, 'Correo sospechoso recibido',          'Llegó un correo que pide la clave del banco.',            '2026-11-21', 4, 5),
 (6, 'La pantalla parpadea',                'El monitor parpadea al abrir cualquier programa.',        '2026-11-22', 2, 1);

INSERT INTO seguimientos (seguimiento_id, ticket_id, tecnico_id, fecha, estado, comentario) VALUES
 (1, 1, 3, '2026-11-16', 'En proceso', 'Se revisa el cable de energía de la impresora.'),
 (2, 1, 3, '2026-11-17', 'Resuelto',   'Se reemplazó el cable de alimentación.'),
 (3, 2, 2, '2026-11-18', 'Resuelto',   'Se restableció la contraseña.'),
 (4, 2, 2, '2026-11-19', 'Cerrado',    'La usuaria confirmó que ya ingresa al correo.'),
 (5, 3, 1, '2026-11-19', 'En proceso', 'Se reinició el switch del área.'),
 (6, 4, 2, '2026-11-21', 'En proceso', 'Instalador descargado; falta la licencia.'),
 (7, 5, 1, '2026-11-21', 'En proceso', 'Se bloqueó el remitente y se analiza el equipo.');

-- Ajusta los contadores para que los nuevos registros continúen la numeración
SELECT setval(pg_get_serial_sequence('categorias',   'categoria_id'),   (SELECT max(categoria_id)   FROM categorias));
SELECT setval(pg_get_serial_sequence('usuarios',     'usuario_id'),     (SELECT max(usuario_id)     FROM usuarios));
SELECT setval(pg_get_serial_sequence('tecnicos',     'tecnico_id'),     (SELECT max(tecnico_id)     FROM tecnicos));
SELECT setval(pg_get_serial_sequence('tickets',      'ticket_id'),      (SELECT max(ticket_id)      FROM tickets));
SELECT setval(pg_get_serial_sequence('seguimientos', 'seguimiento_id'), (SELECT max(seguimiento_id) FROM seguimientos));
