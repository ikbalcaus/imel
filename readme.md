Za backend je koristen .NET 7.0
kada budete pokretali trebate nastimati da je startup project Imel.API

Za frontend je koristen React.js uz vite
prvo instalirajte modele pomocu komande "npm i"
za pokretanje frontenda upisite u konzolu "npm run dev"

Pravu bazu podataka nisam koristio, nisam znao koju tacno jer niste naveli u zadacima, pa sam napravio in memory imitaciju baze podataka.

-------------------------------------------

Prvi zadatak sam uradio kompletno, za registraciju i login se koriste Auth endpoint.
Kada uradite registraciju automatski dobijate ulogu "user"
Dodao sam dodatno jedan endpoint "generete-test-users" koji generise 5 admin korisnika; u responsu dobijete emailove i sifre od tih korisnika (da mozete pristupiti admin opcijama).
Ako se ulogujete kao user necete moci pristupiti crud operacijama i logeru.
Ako budete koristili swagger za crud operaciju i logger, vratit ce vam status kod 401 ili 403.

Drugi zadatak sam takodjer uradio kompletno.
Nakon sto se ulogujete kao admin imate opciju "admin panel".
Na vrhu stranice imate opcije za export podataka.
Na stranici se nalazi DevExpress tabela. Na desnoj strani imate opcije za dodavanje, editovanje i brisanje korisnika.
Ispod se nalazi paginacija. Stavio sam da maksimalno dva korisnika budu na stranici radi lakseg tesitranja.
Filtriranje je omoguceno u DevExpress tabeli, kada kliknete na celiju ispod celije "Active"
Ako kliknete unutar nekog reda u tabeli navigirat ce vas na drugu stranicu gdje se prikazuju sve izmjene (verzije) tog korisnika.

Treci zadatak sam takodjer uradio kompletno (bar se nadam :D ).
Ako sada kliknete unutar reda prikazuje se dijalog koji vas pita da li zelite da vratite promjene. Ako kliknete da, vratite korisnicke podatke na tu verziju.
Napravio sam paginaciju na serveru za ucitavanje verzija korisnika za bolju optimizaciju.
Implementirao sam loger, njega mozete da otvorite na pocetnoj stranici kada kliknete "Audit Logs". Nisam nista htio posebno da dizajniram za tu stranicu.

Dodatno sam napravio "Soft Delete" opciju tako da se korisnik nikada ne brise stvarno, a ako zelite da ponovo omogucite korisnika, samo uradite neku izmjenu na njemu.