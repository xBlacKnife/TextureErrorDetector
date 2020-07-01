call %USERPROFILE%\Anaconda3\Scripts\activate.bat %USERPROFILE%\Anaconda3

@ECHO OFF
cd ./Error Detector

ECHO "Buscando patrones en las imagenes..."
python error_detector.py replanet

cd ..
cd ./ImageCompare
ECHO "Buscando diferencias entre imagenes..."
python ImageComparator.py