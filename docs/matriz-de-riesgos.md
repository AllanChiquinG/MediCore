# Matriz de riesgos — MediCore

**Proyecto:** MediCore, plataforma hospitalaria y de atención médica
**Curso:** 048 Aseguramiento de la Calidad del Software (UMG)
**Versión:** 0.1 (borrador) · **Fecha:** 2026-10-08

## Escala
- **Probabilidad (P):** 1 Baja · 2 Media · 3 Alta
- **Impacto (I):** 1 Bajo · 2 Medio · 3 Alto
- **Nivel = P × I:** 1–2 Bajo · 3–4 Medio · 6 Alto · 9 Crítico
- **Dimensión:** C = Confidencialidad, I = Integridad, D = Disponibilidad

## Riesgos

| ID | Dim. | Riesgo | P | I | Nivel | Mitigación | Prueba que lo cubre |
|----|------|--------|---|---|-------|------------|---------------------|
| R01 | C | Un paciente accede al expediente, citas o resultados de otro paciente (IDOR) | 3 | 3 | 9 Crítico | Autorización a nivel de recurso en cada endpoint | API: GET /api/patients/{id} con token de otro paciente debe dar 403 |
| R02 | C | Un rol no clínico (administrativo) ve diagnósticos o resultados de laboratorio | 2 | 3 | 6 Alto | RBAC por rol: Admin, Médico, Paciente, Laboratorio | API y Cypress: pruebas de autorización por rol |
| R03 | C | Exposición de datos sensibles en respuestas, logs o mensajes de error | 2 | 3 | 6 Alto | DTOs sin campos internos, logs sin datos clínicos, errores genéricos | Revisión de respuestas API, ZAP, SonarQube |
| R04 | C | Robo o reutilización de token/sesión; JWT sin expiración | 2 | 3 | 6 Alto | JWT con expiración corta, sesiones en Redis, cierre de sesión | API: token vencido/manipulado debe dar 401 |
| R05 | C | Secretos (claves, contraseñas) en el repositorio público | 2 | 3 | 6 Alto | `.env` fuera del repo, variables de entorno, escaneo de secretos | Revisión del repo, Dependency-Check, SonarQube |
| R06 | C | Inyección (SQL) o XSS en formularios y parámetros | 2 | 3 | 6 Alto | Consultas parametrizadas (EF Core), validación y codificación de salida | OWASP ZAP, Insomnia con cargas maliciosas |
| R07 | I | Doble reserva de la misma cita médica por solicitudes simultáneas | 3 | 3 | 9 Crítico | Restricción única médico+horario en BD y transacciones | Concurrencia con k6/JMeter + prueba de integración |
| R08 | I | Colisión de horarios del médico (citas solapadas) | 2 | 3 | 6 Alto | Validación de solapamiento en servicio y en BD | Pruebas unitarias e integración |
| R09 | I | Resultado de laboratorio asociado al paciente equivocado o alterado | 1 | 3 | 3 Medio | Validación de paciente/orden, auditoría de cambios | Pruebas de API y de integración |
| R10 | I | Receta modificada o duplicada sin trazabilidad | 2 | 3 | 6 Alto | Recetas inmutables una vez emitidas, registro de auditoría | Pruebas de API, revisión de bitácora |
| R11 | I | Falta de auditoría: no se sabe quién consultó o modificó un expediente | 2 | 2 | 4 Medio | Bitácora de accesos y cambios | Verificación de registros de auditoría |
| R12 | I | Caché desactualizada (Redis) muestra horarios ya ocupados | 3 | 2 | 6 Alto | TTL corto e invalidación al crear o cancelar cita | Pruebas de HIT/MISS, TTL e invalidación |
| R13 | D | Se cae RabbitMQ después de guardar un resultado: se pierde la notificación | 2 | 3 | 6 Alto | Guardar en BD antes de publicar (patrón outbox), reintentos y DLQ | Prueba: detener RabbitMQ tras registrar resultado y verificar que el dato persiste |
| R14 | D | Redis no disponible y el sistema deja de responder | 2 | 2 | 4 Medio | Degradación controlada: consultar BD si falla Redis | Prueba: apagar Redis y verificar respuesta |
| R15 | D | Alta demanda de citas degrada o tumba el sistema | 2 | 3 | 6 Alto | Paginación, caché, límites de tasa | Pruebas de carga y estrés: p95, tasa de error, throughput |
| R16 | D | Caída de base de datos o contenedor sin recuperación | 1 | 3 | 3 Medio | Healthchecks, reinicio automático, respaldos | Prueba de resiliencia en ambiente QA |
| R17 | D | Despliegue con fallas críticas llega a producción | 2 | 3 | 6 Alto | Quality Gate de SonarQube y criterios de salida en Jenkins | Pipeline: etapa de aprobación y Quality Gate |
| R18 | — | Aplicación móvil y web inaccesibles o poco usables | 2 | 2 | 4 Medio | Revisión de usabilidad y accesibilidad | Lighthouse o axe, pruebas con usuarios |

## Pendiente
- Validar con el equipo qué riesgos faltan y ajustar probabilidades.
- Referenciar cada riesgo en el plan de pruebas (`docs/plan-de-pruebas.md`).
