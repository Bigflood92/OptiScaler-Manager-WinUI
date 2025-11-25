# ?? ACTIVAR GITHUB PAGES - GUÍA VISUAL PASO A PASO

## ?? Tiempo: 2 minutos

---

## ?? PASO 1: Ir a Configuración de Pages

**Abre esta URL en tu navegador:**
```
https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings/pages
```

**O manualmente:**
1. Ve a: https://github.com/Bigflood92/OptiScaler-Manager-WinUI
2. Click en **"Settings"** (pestaña arriba a la derecha)
3. En el menú lateral izquierdo, scroll hasta encontrar **"Pages"**
4. Click en **"Pages"**

---

## ?? PASO 2: Configurar Source

Verás una página con el título: **"GitHub Pages"**

### Busca la sección: **"Build and deployment"**

Debajo verás:

**Source:** (un dropdown)
- Click en el dropdown
- Seleccionar: **"Deploy from a branch"**

---

## ?? PASO 3: Seleccionar Branch y Folder

Verás dos nuevos dropdowns que aparecen:

### **Branch:**
- Click en el dropdown
- Seleccionar: **"master"** (o "main" si aparece ese)

### **Folder:**
- Click en el segundo dropdown
- Seleccionar: **"/docs"** (NO "/ (root)")

Debe quedar así:
```
Branch: [master ?]  [/docs ?]
```

---

## ?? PASO 4: Guardar

Click en el botón **"Save"** (al lado de los dropdowns)

---

## ?? PASO 5: Esperar Deployment

Después de guardar, verás un mensaje:

```
? Your site is ready to be published from the docs folder of the master branch.
```

**Espera 1-2 minutos.** La página se recargará automáticamente.

Cuando esté listo, verás:

```
? Your site is live at https://bigflood92.github.io/OptiScaler-Manager-WinUI/
   Visit site ?
```

---

## ?? PASO 6: Verificar que Funciona

**Abre estas URLs en tu navegador:**

### Landing Page:
```
https://bigflood92.github.io/OptiScaler-Manager-WinUI/
```
**Debe mostrar:** Página con fondo oscuro, logo "OptiScaler Manager" en degradado morado/azul

### Privacy Policy:
```
https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html
```
**Debe mostrar:** Página con Privacy Policy completa, fondo oscuro, texto organizado

---

## ? VERIFICACIÓN EXITOSA:

Si ambas URLs cargan correctamente con el diseño bonito:

**? GitHub Pages está ACTIVO y FUNCIONANDO!**

---

## ?? SI HAY PROBLEMAS:

### Error 404 - "Page not found"
- **Causa:** Pages aún no deployed (espera 1-2 min más)
- **Solución:** Refrescar en 30 segundos

### Página sin estilos (solo texto plano)
- **Causa:** CSS no cargó
- **Solución:** Verificar que los archivos HTML estén en /docs
- **Check:** https://github.com/Bigflood92/OptiScaler-Manager-WinUI/tree/master/docs

### No aparece opción "Pages" en Settings
- **Causa:** Repo privado sin upgrade (raro)
- **Solución:** Pages funciona en repos privados gratis desde 2021
- **Check:** Asegurar que estás logueado en GitHub

---

## ?? CAPTURA DE PANTALLA DE CÓMO DEBE VERSE:

### Settings ? Pages debe mostrar:

```
???????????????????????????????????????????????????
? GitHub Pages                                    ?
???????????????????????????????????????????????????
?                                                 ?
? Build and deployment                            ?
?                                                 ?
? Source                                          ?
? [Deploy from a branch        ?]                ?
?                                                 ?
? Branch                                          ?
? [master ?]  [/docs ?]  [Save]                  ?
?                                                 ?
? ? Your site is live at                        ?
? https://bigflood92.github.io/OptiScaler-...    ?
?                                                 ?
? [Visit site]                                    ?
???????????????????????????????????????????????????
```

---

## ?? TIMELINE:

- **0:00** - Abrir Settings ? Pages
- **0:30** - Seleccionar master + /docs
- **0:40** - Click Save
- **0:45** - Mensaje "Ready to be published"
- **2:00** - ? "Your site is live"
- **2:30** - Verificar URLs funcionan

**Total: 2-3 minutos**

---

## ?? DESPUÉS DE ACTIVAR:

**Cuando veas "Your site is live", avísame y continuamos con:**

1. ? Optimizar PNG assets (TinyPNG - 5 min)
2. ?? Capturar screenshots (2-3 hrs)
3. ?? Testing navegación (3-4 hrs)

---

**?? Abre la URL de Settings ahora y sigue los pasos!**

```
https://github.com/Bigflood92/OptiScaler-Manager-WinUI/settings/pages
```
