# OPCIONES DE CERTIFICADOS PARA MSIX

RESUMEN: Para OptiScaler Manager, Microsoft Store (19 USD one-time) es la mejor opcion. Certificado GRATIS incluido.

## OPCION 1: MICROSOFT STORE (RECOMENDADO)

- Costo: 19 USD one-time
- Certificado: GRATIS (Microsoft lo provee)
- Renovaciones: NINGUNA
- Total 5 años: 19 USD

Ventajas:
- Certificado gratis y automatico
- Maxima confianza
- Sin advertencias
- Updates automaticos

## OPCION 2: SELF-SIGNED (TESTING)

- Costo: GRATIS
- Solo para testing local
- Ya creado con Sign-MSIXPackage.ps1

## OPCION 3: CODE SIGNING CA (NO RECOMENDADO)

- Costo: 100-500 USD/año
- Innecesario si usas Store
- Solo para distribucion fuera de Store

## RECOMENDACION:

1. AHORA: Testea con self-signed (Install-Package.ps1)
2. LANZAMIENTO: Publica en Store (Microsoft firma gratis)
3. NO compres certificado Code Signing

