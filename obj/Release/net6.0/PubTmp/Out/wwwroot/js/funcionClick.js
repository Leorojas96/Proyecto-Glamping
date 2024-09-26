const nav = document.querySelector("#nav");
const abrir = document.querySelector("#abrir");
const cerrar = document.querySelector("#cerrar");

abrir.addEventListener("click", () => {
    nav.classList.add("visible");
});

cerrar.addEventListener("click", () => {
    nav.classList.remove("visible");
});


let lastScrollTop = 0; // Variable para rastrear la última posición del scroll
const header = document.querySelector('header'); // Selecciona el header

window.addEventListener('scroll', function() {
    let scrollTop = window.pageYOffset || document.documentElement.scrollTop; // Posición actual del scroll

    if (scrollTop > lastScrollTop) {
        // Si el usuario está desplazándose hacia abajo
        header.style.top = '-100px'; // Oculta el header (ajusta el valor según la altura de tu header)
    } else {
        // Si el usuario está desplazándose hacia arriba
        header.style.top = '0'; // Muestra el header
    }

    lastScrollTop = scrollTop; // Actualiza la última posición del scroll
});

